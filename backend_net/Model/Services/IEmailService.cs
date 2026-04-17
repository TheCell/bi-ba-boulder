using System.Threading.Tasks;

namespace Thecell.Bibaboulder.Model.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string? bcc = null);
}
