using System;

namespace PostIQ.Identity.Models
{
    public enum SecurityTokenKind
    {
        EmailConfirmation = 0,
        PasswordReset = 1,
        PhoneConfirmation = 2
    }

    public class SecurityToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public SecurityTokenKind Kind { get; set; }
        public string TokenHash { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
