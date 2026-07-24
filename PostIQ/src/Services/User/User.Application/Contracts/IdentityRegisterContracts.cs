using System;

namespace User.Application.Contracts
{
    /// <summary>
    /// Payload sent to the Identity service POST /api/auth/register endpoint.
    /// </summary>
    public class IdentityRegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Response body returned by the Identity service on a successful registration.
    /// </summary>
    public class IdentityRegisterResponse
    {
        public Guid UserId { get; set; }
    }

    /// <summary>
    /// ProblemDetails body returned by the Identity service on failures, used to surface a meaningful message.
    /// </summary>
    public class ProblemDetails
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
    }
}
