using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

public class NoOpEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string htmlBody, string? bcc = null)
    {
        // In production, replace with an actual SMTP or API-based email service.
        // This is a no-op placeholder for development.
        Console.WriteLine($"[NoOpEmailService] Simulating sending email to: {to}, subject: {subject}");
        return Task.CompletedTask;
    }
}
