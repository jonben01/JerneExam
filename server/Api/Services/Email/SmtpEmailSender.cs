using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Api.Services.Email;

public class SmtpOptions
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    
    public string FromAddress { get; set; } = null!;
    public string FromDisplayName { get; set; } = "No-Reply";
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };

        using var message = new MailMessage()
        {
            From = new MailAddress(_options.FromAddress, _options.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        
        message.To.Add(new MailAddress(to));
        await client.SendMailAsync(message);
    }
}