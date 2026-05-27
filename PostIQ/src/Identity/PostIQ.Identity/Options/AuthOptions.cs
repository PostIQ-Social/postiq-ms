namespace PostIQ.Identity.Options
{
    public class AuthOptions
    {
        public const string SectionName = "Auth";
        public bool RequireConfirmedEmail { get; set; }
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;
    }
}
