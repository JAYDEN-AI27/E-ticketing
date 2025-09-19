using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendEmail(string to, string subject, string body)
    {
        var smtpSection = _config.GetSection("Smtp");

        // Get values safely with validation
        var fromEmail = smtpSection["User"];
        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("SMTP User email is not configured.");

        var fromPass = smtpSection["Pass"];
        if (string.IsNullOrWhiteSpace(fromPass))
            throw new InvalidOperationException("SMTP password is not configured.");

        var displayName = smtpSection["Name"] ?? "Admin";

        var host = smtpSection["Host"];
        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("SMTP host is not configured.");

        var portString = smtpSection["Port"];
        var port = !string.IsNullOrWhiteSpace(portString) && int.TryParse(portString, out var parsedPort)
            ? parsedPort
            : 587;

        using (var client = new SmtpClient(host, port))
        {
            client.Credentials = new NetworkCredential(fromEmail, fromPass);
            client.EnableSsl = true;

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(fromEmail, displayName);
                mail.To.Add(to);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                client.Send(mail);
            }
        }
    }
}
