using FeeloryBackend.Services.Interfaces;
using FeeloryBackend.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FeeloryBackend.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly EmailSettings _email;

    public EmailService(IOptions<EmailSettings> emailOptions)
    {
        _email = emailOptions.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Feelory", _email.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();
        
        await client.ConnectAsync(
            _email.Host,
            _email.Port,
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            _email.Username,
            _email.Password
        );

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}