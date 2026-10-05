using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace PostIQ.Core.Shared.Email;

public sealed class SmtpEmailSender(IOptions<EmailSenderOptions> options) : IEmailSender
{
    private readonly EmailSenderOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ValidateOptions();

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };
        mailMessage.To.Add(new MailAddress(message.To));

        using var client = new SmtpClient
        {
            Timeout = _options.Timeout,
            EnableSsl = _options.UseSsl || _options.UseStartTls,
            DeliveryMethod = string.IsNullOrWhiteSpace(_options.PickupDirectoryLocation)
                ? SmtpDeliveryMethod.Network
                : SmtpDeliveryMethod.SpecifiedPickupDirectory
        };

        if (client.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
        {
            client.PickupDirectoryLocation = _options.PickupDirectoryLocation;
        }
        else
        {
            client.Host = _options.Host;
            client.Port = _options.Port;
            if (_options.UseAuthentication)
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        await client.SendMailAsync(mailMessage, cancellationToken);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.FromAddress))
            throw new InvalidOperationException("EmailOptions:FromAddress is required.");

        if (string.IsNullOrWhiteSpace(_options.PickupDirectoryLocation))
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new InvalidOperationException("EmailOptions:Host is required.");

            if (_options.Port is < 1 or > 65535)
                throw new InvalidOperationException("EmailOptions:Port must be between 1 and 65535.");

            if (_options.UseAuthentication &&
                (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password)))
            {
                throw new InvalidOperationException(
                    "EmailOptions:Username and EmailOptions:Password are required when authentication is enabled.");
            }
        }

        if (_options.Timeout <= 0)
            throw new InvalidOperationException("EmailOptions:Timeout must be greater than zero.");
    }
}
