using System.ComponentModel.DataAnnotations;
using MediatR;
using PostIQ.Core.Response;
using User.Application.Response;

namespace User.Application.Commands
{
    /// <summary>
    /// Registers a new user: creates the auth account in the Identity service,
    /// stores the user profile, records the referral and rotates the referrer's code.
    /// </summary>
    public class RegisterUserCommand : IRequest<SingleResponse<RegisterUserResponse>>
    {
        [Required, EmailAddress, MaxLength(320)]
        public string Email { get; set; } = null!;

        [Required, MinLength(6), MaxLength(200)]
        public string Password { get; set; } = null!;

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; } = null!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(10)]
        public string ReferralCode { get; set; } = null!;
    }
}
