using MediatR;
using Microsoft.Extensions.Logging;
using PostIQ.Core.Database;
using PostIQ.Core.HttpClientService.Extensions;
using PostIQ.Core.HttpClientService.Services;
using PostIQ.Core.Response;
using PostIQ.Core.Shared.Encrypt;
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
        private const int ReferralCodeLength = 8;

        private readonly IUnitOfWork<UserDBContext> _uow;
        private readonly IRepositoryAsync<UserDetail> _userRepo;
        private readonly IRepositoryAsync<UserReferral> _referralRepo;
        private readonly ILogger<RegisterUserHandler> _logger;

        public RegisterUserHandler(
            IUnitOfWork<UserDBContext> uow,
            IBaseHttpClientService clientService,
            ILogger<RegisterUserHandler> logger)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _userRepo = uow.GetRepositoryAsync<UserDetail>();
            _referralRepo = uow.GetRepositoryAsync<UserReferral>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SingleResponse<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var referralCode = request.ReferralCode.Trim().ToUpperInvariant();

            // 1. The referral code must belong to an active user.
            var referrer = await _userRepo.SingleOrDefaultAsync(x => x.ReferralCode.ToLower() == referralCode.ToLower() && x.IsActive);

            if (referrer == null)
            {
                return Failure(400, "Invalid or already used referral code.");
            }
            var atIndex = request.Email.IndexOf('@');
            var userName = atIndex > 0 ? request.Email[..atIndex] : request.Email;

            // 2. Create the auth account in the Identity service.
            var PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            //var authId = identityResult.UserId;
            var now = DateTime.UtcNow;

            // 3-5. Persist profile, referral record and rotate the referrer's code atomically.
            await using var transaction = await _uow.Context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var newUser = new UserDetail
                {
                    AuthId = request.AuthId,
                    FirstName = request.FirstName.Trim(),
                    MiddleName = string.IsNullOrWhiteSpace(request.MiddleName) ? null : request.MiddleName.Trim(),
                    LastName = request.LastName.Trim(),
                    Phone = PhoneNumber,
                    ReferralCode = RandomGenerator.RandomOTP(8),
                    IsActive = true,
                    CreatedOn = now,
                    CreatedBy = referrer.UserId,
                };

                await _userRepo.InsertAsync(newUser, cancellationToken);
                await _uow.CommitAsync();

                var referral = new UserReferral
                {
                    UserId = newUser.UserId,
                    UserName = $"{newUser.FirstName} {newUser.LastName}".Trim(),
                    ReferralCode = referralCode,
                    ReferredById = referrer.UserId,
                    ReferredByName = $"{referrer.FirstName} {referrer.MiddleName} {referrer.LastName}".Trim(),
                    IsActive = true,
                    CreatedOn = now,
                    CreatedBy = referrer.UserId,
                };

                await _referralRepo.InsertAsync(referral, cancellationToken);

                // Rotate the referrer's code so the same code cannot be reused.
                referrer.ReferralCode = RandomGenerator.RandomOTP(8);
                referrer.UpdatedOn = now;
                referrer.UpdatedBy = newUser.UserId;

                await _uow.CommitAsync();
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Registered user {UserId} (auth {AuthId}) referred by {ReferrerId}.", newUser.UserId, request.AuthId, referrer.UserId);

                return new SingleResponse<RegisterUserResponse>(
                    new RegisterUserResponse(newUser.UserId, request.AuthId, newUser.ReferralCode));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to persist registration for auth {AuthId}.", request.AuthId);
                return Failure(500, "Registration could not be completed. Please try again.");
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
