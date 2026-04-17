using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Spraywall.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FeedbacksController : ControllerBase
{
    private readonly ICommandHandler<SendFeedbackCommand> _sendFeedbackCommandHandler;

    public FeedbacksController(ICommandHandler<SendFeedbackCommand> sendFeedbackCommandHandler)
    {
        _sendFeedbackCommandHandler = sendFeedbackCommandHandler;
    }

    [HttpPost("send")]
    [Authorize(Roles = AuthorizationRoles.User)]
    public async Task<IActionResult> SendFeedback([FromBody] SendFeedbackCommand command)
    {
        await _sendFeedbackCommandHandler.HandleAsync(command);
        return Ok();
    }
}
