namespace PostIQ.Identity.Options
{
    public sealed class PasswordResetOptions
    {
        public const string SectionName = "PasswordReset";
        public string ResetUrl { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 60;
        public string SupportEmail { get; set; } = string.Empty;
    }
}
