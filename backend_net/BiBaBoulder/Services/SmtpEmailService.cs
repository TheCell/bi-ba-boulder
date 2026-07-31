using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Thecell.Bibaboulder.Common.Appsettings;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

public class SmtpEmailService : IEmailService
{
    private static readonly Action<ILogger, string, string, int, bool, bool, string, Exception?> _smtpSendFailed =
        LoggerMessage.Define<string, string, int, bool, bool, string>(
            LogLevel.Error,
            new EventId(1001, nameof(_smtpSendFailed)),
            "SMTP send failed for recipient {Recipient}. Host={Host}, Port={Port}, UseSsl={UseSsl}, UseStartTls={UseStartTls}, Username={Username}");

    private readonly EmailSettings _settings;
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly bool _requiresAuthentication;

    public SmtpEmailService(
        IOptions<EmailSettings> settings,
        IBiBaBoulderDbContext dbContext,
        ILogger<SmtpEmailService> logger,
        IHostEnvironment hostEnvironment)
    {
        _settings = settings.Value;
        _dbContext = dbContext;
        _logger = logger;
        _requiresAuthentication = !hostEnvironment.IsDevelopment();

        if (_settings.UseSsl && _settings.UseStartTls)
        {
            throw new InvalidOperationException("Email settings are invalid: UseSsl and UseStartTls cannot both be true.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            throw new InvalidOperationException("Email settings are invalid: Host must be configured.");
        }

        if (_settings.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("Email settings are invalid: Port must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            throw new InvalidOperationException("Email settings are invalid: FromAddress must be configured.");
        }

        if (_requiresAuthentication && (string.IsNullOrWhiteSpace(_settings.Username) || string.IsNullOrWhiteSpace(_settings.Password)))
        {
            throw new InvalidOperationException("Email settings are invalid: Username and Password must be configured for SMTP authentication.");
        }
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
        catch (Exception ex)
        {
            _smtpSendFailed(
                _logger,
                to,
                _settings.Host,
                _settings.Port,
                _settings.UseSsl,
                _settings.UseStartTls,
                _settings.Username,
                ex);
            await _dbContext.RemoveEntityAndSaveChangesAsync(mail);
            throw;
        }
    }
}
