using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var host = _configuration["EmailSettings:Host"];
        var portValue = _configuration["EmailSettings:Port"];
        var username = _configuration["EmailSettings:UserName"];
        var password = _configuration["EmailSettings:Password"];
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portValue) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(senderEmail))
        {
            return;
        }

        using var client = new SmtpClient(host, int.Parse(portValue))
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName ?? senderEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);
        await client.SendMailAsync(message, ct);
    }
}
