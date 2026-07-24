using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OBS.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var smtp = _config.GetSection("SmtpSettings");
        var host     = smtp["Host"]     ?? "smtp.gmail.com";
        var port     = int.Parse(smtp["Port"] ?? "587");
        var username = smtp["Username"] ?? "";
        var password = smtp["Password"] ?? "";
        var fromName = smtp["FromName"] ?? "OBS Sistemi";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, username));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }

        _logger.LogInformation("Mail gönderildi: {To}", toEmail);
    }
}
