using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PostIQ.Identity.Models;
using PostIQ.Identity.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace PostIQ.Identity.Services
{
    public sealed class JwtTokenService(IOptions<JwtOptions> options)
    {
        public const string PurposeClaim = "purpose";
        public const string TwoFactorPendingPurpose = "2fa_pending";

        private readonly JwtOptions _opt = options.Value;

        private SymmetricSecurityKey SigningKey =>
            new(Encoding.UTF8.GetBytes(_opt.SigningKey));

        private TokenValidationParameters ValidationParameters => new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SigningKey,
            ValidateIssuer = true,
            ValidIssuer = _opt.Issuer,
            ValidateAudience = true,
            ValidAudience = _opt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        public string CreateAccessToken(User user)
        {
            var roles = ParseRoles(user.Roles);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new("email_verified", user.EmailConfirmed ? "true" : "false"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            if (!string.IsNullOrEmpty(user.UserName))
            {
                claims.Add(new Claim("preferred_username", user.UserName));
            }

            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            return CreateJwt(claims, TimeSpan.FromMinutes(_opt.AccessTokenMinutes));
        }

        public string CreatePendingTwoFactorToken(Guid userId)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(PurposeClaim, TwoFactorPendingPurpose),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            return CreateJwt(claims, TimeSpan.FromMinutes(_opt.PendingTwoFactorMinutes));
        }

        public bool TryValidatePendingTwoFactorToken(string token, out Guid userId)
        {
            userId = default;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, ValidationParameters, out var validated);
                var purpose = principal.FindFirst(PurposeClaim)?.Value;

                if (purpose != TwoFactorPendingPurpose)
                {
                    return false;
                }

                var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                return sub != null && Guid.TryParse(sub, out userId);
            }
            catch
            {
                return false;
            }
        }

        private string CreateJwt(IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.Add(lifetime),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static IEnumerable<string> ParseRoles(string roles)
        {
            if (string.IsNullOrWhiteSpace(roles))
            {
                return ["User"];
            }

            return roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}

