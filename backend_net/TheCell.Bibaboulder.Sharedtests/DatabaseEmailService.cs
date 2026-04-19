using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Sharedtests;

public class DatabaseEmailService : IEmailService
{
    private readonly IServiceProvider _serviceProvider;

    public string? LastRecipient { get; private set; }
    public string? LastSubject { get; private set; }
    public string? LastBody { get; private set; }
    public string? LastBcc { get; private set; }
    public int SendCount { get; private set; }

    public DatabaseEmailService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? bcc = null)
    {
        LastRecipient = to;
        LastSubject = subject;
        LastBody = htmlBody;
        LastBcc = bcc;
        SendCount++;

        // Get DbContext from service provider for each call
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IBiBaBoulderDbContext>();

        var mail = new Mail
        {
            Id = Guid.NewGuid(),
            To = to,
            Bcc = bcc,
            Subject = subject,
            Body = htmlBody,
            SentAt = DateTime.UtcNow
        };

        await dbContext.InsertEntityAsync(mail);
        await dbContext.SaveChangesAsync();
    }
}
