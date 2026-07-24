namespace PostIQ.Identity.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string? UserName { get; set; }
        public string PasswordHash { get; set; } = "";
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorSecret { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        //comma seperated roles, e.g. "User,Admin"
        public string Roles { get; set; } = "User";
        public DateTimeOffset CreatedAt { get; set; }
    }
}
