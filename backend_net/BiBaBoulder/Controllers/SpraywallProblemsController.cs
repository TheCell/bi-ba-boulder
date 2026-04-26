using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Spraywall.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SpraywallProblemsController : ControllerBase
{
    private readonly IQueryHandler<GetSpraywallProblemQuery, SpraywallProblemDto> _getSpraywallProblemQueryHandler;
    private readonly ICommandHandler<UpdateSpraywallProblemCommand> _updateProblemCommandHandler;
    private readonly ICommandHandlerWithExtraTransaction<DeleteSpraywallProblemCommand> _deleteProblemCommandHandler;

    public SpraywallProblemsController(
        IQueryHandler<GetSpraywallProblemQuery, SpraywallProblemDto> getSpraywallProblemQueryHandler,
        ICommandHandler<UpdateSpraywallProblemCommand> updateProblemCommandHandler,
        ICommandHandlerWithExtraTransaction<DeleteSpraywallProblemCommand> deleteProblemCommandHandler)
    {
        _getSpraywallProblemQueryHandler = getSpraywallProblemQueryHandler;
        _updateProblemCommandHandler = updateProblemCommandHandler;
        _deleteProblemCommandHandler = deleteProblemCommandHandler;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<SpraywallProblemDto> GetProblem(Guid id)
    {
        return await _getSpraywallProblemQueryHandler.HandleAsync(
            new GetSpraywallProblemQuery
            {
                Id = id
            });
    }

    [HttpPost("{id}")]
    [Authorize(Roles = AuthorizationRoles.Editor)]
    public async Task<IActionResult> UpdateProblem(Guid id, [FromBody] UpdateSpraywallProblemCommand command)
    {
        command.Id = id;
        await _updateProblemCommandHandler.HandleAsync(command);
        return Ok(new { Message = "Problem updated successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.Editor},{AuthorizationRoles.Admin}")]
    public async Task<IActionResult> DeleteProblem(Guid id)
    {
        await _deleteProblemCommandHandler.HandleAsync(
            new DeleteSpraywallProblemCommand { Id = id });
        return Ok(new { Message = "Problem deleted successfully" });
    }
}
