using System.ComponentModel.DataAnnotations;

namespace PostIQ.Identity.Contracts
{
    public class RegisterRequest
    {
        [Required, EmailAddress, MaxLength(320)]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(6), MaxLength(200)]
        public string Password { get; set; } = string.Empty;
        [MaxLength(256)]
        public string? UserName { get; set; }
        [MaxLength(32)]
        public string? PhoneNumber { get; set; } = null;
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    public class LoginTotpRequest
    {
        [Required]
        public string PendingToken { get; set; } = "";
        [Required, MinLength(6), MaxLength(10)]
        public string Code { get; set; } = string.Empty;

    }
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = "";
    }
    public class RevokeRequest
    {
        [Required]
        public string RefreshToken { get; set; } = "";
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = "";
        [Required, MinLength(6), MaxLength(200)]
        public string NewPassword { get; set; } = "";
    }
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        public string Token { get; set; } = "";
        [Required, MinLength(8), MaxLength(200)]
        public string NewPassword { get; set; } = "";
    }
    public class ConfirmEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        public string Token { get; set; } = "";
    }
    public class ResendConfirmationRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }
    public class EnableTwoFactorRequest
    {
        [Required, MinLength(6), MaxLength(10)]
        public string Code { get; set; } = string.Empty;
    }
    public class DisableTwoFactorRequest
    {
        [Required, MinLength(6), MaxLength(10)]
        public string Code { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    public class RequestPhoneRequest
    {
        [MaxLength(32)]
        public string PhoneNumber { get; set; } = "";
    }
    public class ConfirmPhoneRequest
    {
        [Required, MinLength(6), MaxLength(10)]
        public string Code { get; set; } = string.Empty;
    }
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
    public class TwoFactorRequiredResponse
    {
        public bool RequiredTwoFactor { get; set; } = false;
        public string PendingToken {  get; set; } = string.Empty;
    }
    public record UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? UserName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public IReadOnlyList<string> Roles { get; set; } = [];
    }

    public record TwoFactorSetupResponse
    {
        public string Secret { get; set; } = "";
        public string ManualEntryKey { get; set; } = "";
        public string QrCodeUri { get; set; } = "";
    }

    public record ExternalProvidersResponse
    {
        public string[] Providers { get; set; } = Array.Empty<string>();
    }

}
