namespace PostIQ.Core.Shared.Email;

public sealed class EmailSenderOptions
{
    public const string SectionName = "EmailOptions";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; }
    public bool UseStartTls { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public int Timeout { get; set; } = 100_000;
    public bool UseAuthentication { get; set; } = true;
    public string PickupDirectoryLocation { get; set; } = string.Empty;
}
