using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Thecell.Bibaboulder.Common.Appsettings;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SendFeedbackCommandHandler : ICommandHandler<SendFeedbackCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly IOptions<EmailSettings> _emailSettings;

    public SendFeedbackCommandHandler(
        ICurrentUserService currentUserService,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings)
    {
        _currentUserService = currentUserService;
        _emailService = emailService;
        _emailSettings = emailSettings;
    }

    public async Task HandleAsync(SendFeedbackCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Feedback))
        {
            throw new ArgumentException("Feedback is required and cannot be empty");
        }

        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var subject = $"User Feedback From {currentUser.Email}";
        var body = $"<p>Feedback from <strong>{currentUser.Email}</strong>:</p><p>{command.Feedback}</p>";

        await _emailService.SendEmailAsync(
            _emailSettings.Value.ContactAddress,
            subject,
            body,
            currentUser.Email);
    }
}
