using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Sharedtests;

public class InMemoryEmailService : IEmailService
{
    public string? LastRecipient { get; private set; }
    public string? LastSubject { get; private set; }
    public string? LastBody { get; private set; }
    public int SendCount { get; private set; }

    public Task SendEmailAsync(string to, string subject, string htmlBody, string? bcc = null)
    {
        LastRecipient = to;
        LastSubject = subject;
        LastBody = htmlBody;
        SendCount++;
        return Task.CompletedTask;
    }
}
