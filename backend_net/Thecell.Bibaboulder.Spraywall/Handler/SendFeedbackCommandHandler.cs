using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SendFeedbackCommandHandler : ICommandHandler<SendFeedbackCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public SendFeedbackCommandHandler(
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _currentUserService = currentUserService;
        _emailService = emailService;
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
            currentUser.Email,
            subject,
            body);
    }
}
