using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PostIQ.Identity.Contracts;
using PostIQ.Identity.Data;
using PostIQ.Identity.Models;
using PostIQ.Identity.Options;

namespace PostIQ.Identity.Services
{
    public sealed class AuthService
    {
        private readonly IdentityDbContext db;
        private readonly PasswordHasherService passwordHasher;
        private readonly JwtTokenService jwt;
        private readonly TotpService totp;
        private readonly IOptions<JwtOptions> jwtOptions;
        private readonly IOptions<AuthOptions> authOptions;
        private readonly ILogger<AuthService> logger;
        private readonly IWebHostEnvironment env;

        private readonly JwtOptions _jwtOpt;
        private readonly AuthOptions _authOpt;

        public AuthService(
            IdentityDbContext db,
            PasswordHasherService passwordHasher,
            JwtTokenService jwt,
            TotpService totp,
            IOptions<JwtOptions> jwtOptions,
            IOptions<AuthOptions> authOptions,
            ILogger<AuthService> logger,
            IWebHostEnvironment env)
        {
            this.db = db;
            this.passwordHasher = passwordHasher;
            this.jwt = jwt;
            this.totp = totp;
            this.jwtOptions = jwtOptions;
            this.authOptions = authOptions;
            this.logger = logger;
            this.env = env;

            _jwtOpt = jwtOptions.Value;
            _authOpt = authOptions.Value;
        }

        public async Task<Result<Guid>> RegisterAsync(RegisterRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(u => u.Email == email, ct))
                return Result<Guid>.Failure(409, "Email is already registered.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = string.IsNullOrWhiteSpace(req.UserName) ? null : req.UserName.Trim(),
                PasswordHash = passwordHasher.HashPassword(req.Password),
            };

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            await CreateEmailConfirmationTokenAsync(user.Id, ct);
            return Result<Guid>.Success(user.Id, 201);
        }

        public async Task<Result<object>> LoginAsync(LoginRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
                return Result<object>.Failure(401, "Invalid credentials.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                return Result<object>.Failure(423, "Account is locked. Try again later.");

            if (!passwordHasher.VerifyPassword(req.Password, user.PasswordHash))
            {
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= _authOpt.MaxFailedAccessAttempts)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(_authOpt.LockoutMinutes);
                    user.AccessFailedCount = 0;
                }
                await db.SaveChangesAsync(ct);
                return Result<object>.Failure(401, "Invalid credentials.");
            }

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            await db.SaveChangesAsync(ct);

            if (_authOpt.RequireConfirmedEmail && !user.EmailConfirmed)
                return Result<object>.Failure(403, "Email address is not confirmed.");

            if (user.TwoFactorEnabled)
            {
                var pending = jwt.CreatePendingTwoFactorToken(user.Id);
                return Result<object>.Success(new { PendingToken = pending });
            }

            var tokens = await IssueTokensAsync(user, ct);
            return Result<object>.Success(tokens);
        }

        public async Task<Result<TokenResponse>> CompleteTotpLoginAsync(LoginTotpRequest req, CancellationToken ct)
        {
            if (!jwt.TryValidatePendingTwoFactorToken(req.PendingToken, out var userId))
                return Result<TokenResponse>.Failure(401, "Invalid or expired pending token.");

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return Result<TokenResponse>.Failure(401, "Two-factor authentication is not active for this account.");

            if (!totp.ValidateCode(user.TwoFactorSecret, req.Code))
                return Result<TokenResponse>.Failure(401, "Invalid authenticator code.");

            var tokens = await IssueTokensAsync(user, ct);
            return Result<TokenResponse>.Success(tokens);
        }

        public async Task<Result<TokenResponse>> RefreshAsync(RefreshRequest req, CancellationToken ct)
        {
            var hash = CryptoUtil.Sha256Hex(req.RefreshToken);
            var existing = await db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

            if (existing is null || existing.RevokedAt.HasValue || existing.ExpiresAt < DateTimeOffset.UtcNow)
                return Result<TokenResponse>.Failure(401, "Invalid or expired refresh token.");

            existing.RevokedAt = DateTimeOffset.UtcNow;
            var rotated = await IssueTokensAsync(existing.User, ct);
            existing.ReplacedByTokenHash = CryptoUtil.Sha256Hex(rotated.RefreshToken);
            await db.SaveChangesAsync(ct);

            return Result<TokenResponse>.Success(rotated);
        }

        public async Task<Result<object>> RevokeAsync(RevokeRequest req, CancellationToken ct)
        {
            var hash = CryptoUtil.Sha256Hex(req.RefreshToken);
            var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

            if (token is null)
                return Result<object>.Failure(404, "Refresh token not found.");

            token.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);

            return Result<object>.Success(new { revoked = true });
        }

        public async Task<Result<object>> RevokeAllAsync(Guid userId, CancellationToken ct)
        {
            var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null).ToListAsync(ct);
            var now = DateTimeOffset.UtcNow;
            foreach (var t in tokens)
            {
                t.RevokedAt = now;
            }
            await db.SaveChangesAsync(ct);
            return Result<object>.Success(new { revoked = tokens.Count });
        }

        public async Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId, CancellationToken ct)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<UserProfileResponse>.Failure(404, "User not found.");

            return Result<UserProfileResponse>.Success(MapProfile(user));
        }

        public async Task<Result<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest req, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<object>.Failure(404, "User not found.");

            if (!passwordHasher.VerifyPassword(req.CurrentPassword, user.PasswordHash))
                return Result<object>.Failure(400, "Current password is incorrect.");

            user.PasswordHash = passwordHasher.HashPassword(req.NewPassword);
            await RevokeAllRefreshTokensForUserAsync(user.Id, ct);
            await db.SaveChangesAsync(ct);

            return Result<object>.Success(new { passwordChanged = true });
        }

        public async Task<Result<object>> ForgotPasswordAsync(ForgotPasswordRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
            {
                logger.LogInformation("Password reset requested for unknown email (silent success).");
                return Result<object>.Success(new { message = "If the email exists, reset instructions were sent." });
            }

            await db.SecurityTokens.Where(t => t.UserId == user.Id && t.Kind == SecurityTokenKind.PasswordReset)
                .ExecuteDeleteAsync(ct);

            var opaque = CryptoUtil.GenerateOpaqueToken();
            var token = new SecurityToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Kind = SecurityTokenKind.PasswordReset,
                TokenHash = CryptoUtil.Sha256Hex(opaque),
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.SecurityTokens.Add(token);
            await db.SaveChangesAsync(ct);

            logger.LogWarning("Password reset token for {Email}: {Token}", email, opaque);

            var payload = new Dictionary<string, object?>
            {
                ["message"] = "If the email exists, reset instructions were sent."
            };

            if (env.IsDevelopment())
                payload["resetToken"] = opaque;

            return Result<object>.Success(payload);
        }

        public async Task<Result<object>> ResetPasswordAsync(ResetPasswordRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var hash = CryptoUtil.Sha256Hex(req.Token);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
                return Result<object>.Failure(400, "Invalid reset request.");

            var st = await db.SecurityTokens.FirstOrDefaultAsync(t =>
                t.UserId == user.Id &&
                t.Kind == SecurityTokenKind.PasswordReset &&
                t.TokenHash == hash, ct);

            if (st is null || st.ExpiresAt < DateTimeOffset.UtcNow)
                return Result<object>.Failure(400, "Invalid or expired reset token.");

            user.PasswordHash = passwordHasher.HashPassword(req.NewPassword);
            db.SecurityTokens.Remove(st);

            await RevokeAllRefreshTokensForUserAsync(user.Id, ct);
            await db.SaveChangesAsync(ct);

            return Result<object>.Success(new { passwordReset = true });
        }

        public async Task<Result<object>> ConfirmEmailAsync(ConfirmEmailRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var hash = CryptoUtil.Sha256Hex(req.Token);
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

            if (user is null)
                return Result<object>.Failure(400, "Invalid confirmation request.");

            var st = await db.SecurityTokens.FirstOrDefaultAsync(t =>
                t.UserId == user.Id &&
                t.Kind == SecurityTokenKind.EmailConfirmation &&
                t.TokenHash == hash, ct);

            if (st is null || st.ExpiresAt < DateTimeOffset.UtcNow)
                return Result<object>.Failure(400, "Invalid or expired confirmation token.");

            user.EmailConfirmed = true;
            db.SecurityTokens.Remove(st);
            await db.SaveChangesAsync(ct);

            return Result<object>.Success(new { emailConfirmed = true });
        }

        public async Task<Result<object>> ResendConfirmationAsync(ResendConfirmationRequest req, CancellationToken ct)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            
            if (user is null || user.EmailConfirmed)
                return Result<object>.Success(new { message = "If the email exists, confirmation instructions were sent." });

            // Logic to resend confirmation email goes here
            await db.SecurityTokens.Where(t => t.UserId == user.Id && t.Kind == SecurityTokenKind.EmailConfirmation)
                .ExecuteDeleteAsync(ct);
            await CreateEmailConfirmationTokenAsync(user.Id, ct);

            return Result<object>.Success(new { message = "If the email exists, confirmation instructions were sent." });
        }

        public async Task<Result<TwoFactorSetupResponse>> SetupTwoFactorAsync(Guid userId, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<TwoFactorSetupResponse>.Failure(404, "User not found.");  
            if (user.TwoFactorEnabled)
                return Result<TwoFactorSetupResponse>.Failure(400, "Two-factor authentication is already enabled.");

            var secret = totp.GenerateBase32Secret();
            user.TwoFactorSecret = secret;
            await db.SaveChangesAsync(ct);

            var issuer = "IdentityService";
            var account = user.UserName ?? user.Email;
            var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(account)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits=6";
            return Result<TwoFactorSetupResponse>.Success(new TwoFactorSetupResponse
            {
                Secret = secret,
                ManualEntryKey = secret,
                QrCodeUri = uri
            });
        }

        public async Task<Result<object>> EnableTotpAsync(Guid userId, EnableTwoFactorRequest req, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<object>.Failure(404, "User not found.");

            if (string.IsNullOrEmpty(user.TwoFactorSecret))
                return Result<object>.Failure(400, "TOTP setup has not been initiated.");

            if (!totp.ValidateCode(user.TwoFactorSecret, req.Code))
                return Result<object>.Failure(400, "Invalid verification code.");

            user.TwoFactorEnabled = true;
            await RevokeAllRefreshTokensForUserAsync(user.Id, ct);
            await db.SaveChangesAsync(ct);

            return Result<object>.Success(new { enabled = true });
        }

        public async Task<Result<object>> DisableTotpAsync(Guid userId, DisableTwoFactorRequest req, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<object>.Failure(404, "User not found.");
            if(!user.TwoFactorEnabled)
                return Result<object>.Failure(400, "Two-factor authentication is not enabled.");
            if(!passwordHasher.VerifyPassword(req.Password, user.PasswordHash))
                return Result<object>.Failure(400, "Current password is incorrect.");
            if(!string.IsNullOrEmpty(user.TwoFactorSecret) && !string.IsNullOrEmpty(req.Code))
            {
                if (!totp.ValidateCode(user.TwoFactorSecret, req.Code))
                    return Result<object>.Failure(400, "Invalid verification code.");
            }

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;

            await RevokeAllRefreshTokensForUserAsync(user.Id, ct);
            await db.SaveChangesAsync(ct);

            return Result<object>.Success(new { disabled = true });
        }

        public async Task<Result<object>> RequestPhoneVerificationAsync(Guid userId, RequestPhoneRequest req, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<object>.Failure(404, "User not found.");

            await db.SecurityTokens.Where(t => t.UserId == user.Id && t.Kind == SecurityTokenKind.PhoneConfirmation)
                .ExecuteDeleteAsync(ct);

            var code = Random.Shared.Next(0, 1_000_000).ToString("D6");
            var token = new SecurityToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Kind = SecurityTokenKind.PhoneConfirmation,
                TokenHash = CryptoUtil.Sha256Hex($"{user.Id:N}{code}"),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                CreatedAt = DateTimeOffset.UtcNow
            };

            user.PhoneNumber = req.PhoneNumber.Trim();
            user.PhoneNumberConfirmed = false;
            db.SecurityTokens.Add(token);
            await db.SaveChangesAsync(ct);

            logger.LogWarning("SMS verification code for user {UserId} phone {Phone}: {Code}", userId, req.PhoneNumber, code);
            var body = new Dictionary<string, object?> { ["message"] = "Verification code sent (see server logs in development)." };
            if (env.IsDevelopment())
            {
                body["devCode"] = code;
            }
            return Result<object>.Success(body);
        }

        public async Task<Result<object>> ConfirmPhoneAsync(Guid userId, ConfirmPhoneRequest req, CancellationToken ct)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                return Result<object>.Failure(404, "User not found.");

            var hash = CryptoUtil.Sha256Hex($"{user.Id:N}{req.Code.Trim()}");
            var st = await db.SecurityTokens.FirstOrDefaultAsync(
                t => t.UserId == user.Id && t.Kind == SecurityTokenKind.PhoneConfirmation && t.TokenHash == hash,
                ct);

            if (st is null || st.ExpiresAt < DateTimeOffset.UtcNow)
                return Result<object>.Failure(400, "Invalid or expired code.");

            user.PhoneNumberConfirmed = true;
            db.SecurityTokens.Remove(st);
            await db.SaveChangesAsync(ct);
            return Result<object>.Success(new { phoneConfirmed = true });
        }

        private async Task CreateEmailConfirmationTokenAsync(Guid userId, CancellationToken ct)
        {
            var opaque = CryptoUtil.GenerateOpaqueToken();
            var token = new SecurityToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Kind = SecurityTokenKind.EmailConfirmation,
                TokenHash = CryptoUtil.Sha256Hex(opaque),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.SecurityTokens.Add(token);
            await db.SaveChangesAsync(ct);

            var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
            logger.LogWarning("Email confirmation for {Email}: {Token}", user.Email, opaque);
        }

        private async Task RevokeAllRefreshTokensForUserAsync(Guid userId, CancellationToken ct)
        {
            var tokens = await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null).ToListAsync(ct);
            var now = DateTimeOffset.UtcNow;
            foreach (var t in tokens)
            {
                t.RevokedAt = now;
            }
        }

        private async Task<TokenResponse> IssueTokensAsync(User user, CancellationToken ct)
        {
            var access = jwt.CreateAccessToken(user);
            var rawRefresh = CryptoUtil.GenerateOpaqueToken();
            var refreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = CryptoUtil.Sha256Hex(rawRefresh),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtOpt.RefreshTokenDays),
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.RefreshTokens.Add(refreshEntity);
            await db.SaveChangesAsync(ct);

            return new TokenResponse
            {
                AccessToken = access,
                RefreshToken = rawRefresh,
                ExpiresIn = _jwtOpt.AccessTokenMinutes * 60,
                TokenType = "Bearer"
            };
        }


        private static UserProfileResponse MapProfile(User user)
        {
            return new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                EmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Roles = user.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>()
            };
        }
    }
}
