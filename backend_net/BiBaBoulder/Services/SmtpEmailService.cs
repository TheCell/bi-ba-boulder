using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Thecell.Bibaboulder.Common.Appsettings;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IBiBaBoulderDbContext _dbContext;

    public SmtpEmailService(IOptions<EmailSettings> settings, IBiBaBoulderDbContext dbContext)
    {
        _settings = settings.Value;
        _dbContext = dbContext;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? replyTo = null, string? bcc = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        if (!string.IsNullOrEmpty(replyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(replyTo));
        }
        message.To.Add(MailboxAddress.Parse(to));

        if (!string.IsNullOrEmpty(bcc))
        {
            message.Bcc.Add(MailboxAddress.Parse(bcc));
        }

        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        var mail = new Email
        {
            Id = Guid.NewGuid(),
            To = to,
            Bcc = bcc,
            Subject = subject,
            Body = htmlBody,
            SentAt = DateTime.UtcNow
        };
        await _dbContext.InsertEntityAndSaveChangesAsync(mail);

        try
        {
            using var client = new SmtpClient();

            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : _settings.UseStartTls
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);

            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception)
        {
            await _dbContext.RemoveEntityAndSaveChangesAsync(mail);
            throw;
        }
    }
}
