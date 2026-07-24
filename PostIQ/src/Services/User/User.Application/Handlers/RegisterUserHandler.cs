using MediatR;
using Microsoft.Extensions.Logging;
using PostIQ.Core.Database;
using PostIQ.Core.HttpClientService.Extensions;
using PostIQ.Core.HttpClientService.Services;
using PostIQ.Core.Response;
using User.Application.Commands;
using User.Application.Contracts;
using User.Application.Response;
using User.Core.Entities;
using User.Core.Persistence;

namespace User.Application.Handlers
{
    /// <summary>
    /// Orchestrates user registration:
    /// 1. Validates the supplied referral code belongs to an active user.
    /// 2. Creates the auth account in the Identity service and receives the AuthId.
    /// 3. Persists the user profile (with a freshly generated referral code).
    /// 4. Records the referral relationship.
    /// 5. Rotates the referrer's referral code so it cannot be reused.
    /// </summary>
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, SingleResponse<RegisterUserResponse>>
    {
        private const string IdentityClientName = "IdentityClient";
        private const string IdentityRegisterPath = "api/auth/register";
        private const string ReferralCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int ReferralCodeLength = 8;

        private readonly IUnitOfWork<UserDBContext> _uow;
        private readonly IRepositoryAsync<UserDetail> _userRepo;
        private readonly IRepositoryAsync<UserReferral> _referralRepo;
        private readonly IBaseHttpClientService _clientService;
        private readonly ILogger<RegisterUserHandler> _logger;

        public RegisterUserHandler(
            IUnitOfWork<UserDBContext> uow,
            IBaseHttpClientService clientService,
            ILogger<RegisterUserHandler> logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userRepo = uow.GetRepositoryAsync<UserDetail>();
            _referralRepo = uow.GetRepositoryAsync<UserReferral>();
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SingleResponse<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var referralCode = request.ReferralCode.Trim().ToUpperInvariant();

            // 1. The referral code must belong to an active user.
            var referrer = await _userRepo.SingleOrDefaultAsync(
                x => x.ReferralCode == referralCode && x.IsActive);

            if (referrer == null)
            {
                return Failure(400, "Invalid or already used referral code.");
            }
            var atIndex = request.Email.IndexOf('@');
            var userName = atIndex > 0 ? request.Email[..atIndex] : request.Email;

            // 2. Create the auth account in the Identity service.
            var identityRequest = new IdentityRegisterRequest
            {
                Email = request.Email.Trim(),
                Password = request.Password,
                UserName = string.IsNullOrWhiteSpace(userName) ? null : userName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            };

            using var identityResponse = await _clientService.PostAsync(
                IdentityClientName, IdentityRegisterPath, identityRequest, cancellationToken: cancellationToken);

            if (!identityResponse.IsSuccessStatusCode)
            {
                var problem = await SafeReadProblemAsync(identityResponse, cancellationToken);
                var message = string.IsNullOrWhiteSpace(problem?.Detail)
                    ? "Unable to create the account. Please try again."
                    : problem!.Detail!;

                _logger.LogWarning("Identity registration failed (status={Status}): {Detail}", identityResponse.StatusCode, message);
                return Failure(identityResponse.StatusCode == 409 ? 409 : 400, message);
            }

            var identityResult = await identityResponse.ToObjectAsync<IdentityRegisterResponse>();
            if (identityResult == null || identityResult.UserId == Guid.Empty)
            {
                _logger.LogError("Identity registration succeeded but returned no auth id.");
                return Failure(502, "Account service returned an invalid response.");
            }

            var authId = identityResult.UserId;
            var now = DateTime.UtcNow;

            // 3-5. Persist profile, referral record and rotate the referrer's code atomically.
            await using var transaction = await _uow.Context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var newUserCode = await GenerateUniqueReferralCodeAsync(cancellationToken);

                var newUser = new UserDetail
                {
                    AuthId = authId,
                    FirstName = request.FirstName.Trim(),
                    MiddleName = string.IsNullOrWhiteSpace(request.MiddleName) ? null : request.MiddleName.Trim(),
                    LastName = request.LastName.Trim(),
                    Phone = identityRequest.PhoneNumber,
                    ReferralCode = newUserCode,
                    IsActive = true,
                    CreatedOn = now,
                    CreatedBy = referrer.UserId,
                };

                await _userRepo.InsertAsync(newUser, cancellationToken);
                await _uow.CommitAsync();

                var referral = new UserReferral
                {
                    UserId = newUser.UserId,
                    UserName = userName,
                    ReferralCode = referralCode,
                    ReferredById = referrer.UserId,
                    ReferredByName = $"{referrer.FirstName} {referrer.MiddleName} {referrer.LastName}".Trim(),
                    IsActive = true,
                    CreatedOn = now,
                    CreatedBy = referrer.UserId,
                };

                await _referralRepo.InsertAsync(referral, cancellationToken);

                // Rotate the referrer's code so the same code cannot be reused.
                referrer.ReferralCode = await GenerateUniqueReferralCodeAsync(cancellationToken);
                referrer.UpdatedOn = now;
                referrer.UpdatedBy = newUser.UserId;

                await _uow.CommitAsync();
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Registered user {UserId} (auth {AuthId}) referred by {ReferrerId}.", newUser.UserId, authId, referrer.UserId);

                return new SingleResponse<RegisterUserResponse>(
                    new RegisterUserResponse(newUser.UserId, authId, newUser.ReferralCode));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to persist registration for auth {AuthId}.", authId);
                return Failure(500, "Registration could not be completed. Please try again.");
            }
        }

        private async Task<string> GenerateUniqueReferralCodeAsync(CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var candidate = GenerateReferralCode();
                var exists = await _userRepo.SingleOrDefaultAsync(
                    x => x.ReferralCode == candidate, enableTracking: false);

                if (exists == null)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException("Unable to generate a unique referral code.");
        }

        private static string GenerateReferralCode()
        {
            var chars = new char[ReferralCodeLength];
            for (var i = 0; i < ReferralCodeLength; i++)
            {
                chars[i] = ReferralCodeChars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(ReferralCodeChars.Length)];
            }

            return new string(chars);
        }

        private static async Task<ProblemDetails?> SafeReadProblemAsync(
            PostIQ.Core.HttpClientService.Models.HttpResponseResult response, CancellationToken cancellationToken)
        {
            try
            {
                return await response.ToObjectAsync<ProblemDetails>();
            }
            catch
            {
                return null;
            }
        }

        private static SingleResponse<RegisterUserResponse> Failure(int statusCode, string message)
        {
            return new SingleResponse<RegisterUserResponse>(null!)
            {
                Errors = new List<KeyValuePair<string, string[]>>
                {
                    new(statusCode.ToString(), new[] { message }),
                },
            };
        }
    }
}
