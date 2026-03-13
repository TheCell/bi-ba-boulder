using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Spraywall.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SpraywallsController : ControllerBase
{
    private readonly IQueryHandler<GetSpraywallsQuery, ICollection<SpraywallDto>> _getSpraywallsQueryHandler;
    private readonly IQueryHandler<SearchSpraywallProblemsQuery, SpraywallProblemListDto> _searchProblemsQueryHandler;
    private readonly ICommandHandler<CreateSpraywallProblemCommand> _createProblemCommandHandler;
    private readonly IQueryHandler<GetSpraywallProblemQuery, SpraywallProblemDto> _getSpraywallProblemQueryHandler;
    private readonly ICurrentUserService _currentUserService;

    public SpraywallsController(
        IQueryHandler<GetSpraywallsQuery, ICollection<SpraywallDto>> getSpraywallsQueryHandler,
        IQueryHandler<SearchSpraywallProblemsQuery, SpraywallProblemListDto> searchProblemsQueryHandler,
        ICommandHandler<CreateSpraywallProblemCommand> createProblemCommandHandler,
        IQueryHandler<GetSpraywallProblemQuery, SpraywallProblemDto> getSpraywallProblemQueryHandler,
        ICurrentUserService currentUserService)
    {
        _getSpraywallsQueryHandler = getSpraywallsQueryHandler;
        _searchProblemsQueryHandler = searchProblemsQueryHandler;
        _createProblemCommandHandler = createProblemCommandHandler;
        _getSpraywallProblemQueryHandler = getSpraywallProblemQueryHandler;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ICollection<SpraywallDto>> GetSpraywalls()
    {
        return await _getSpraywallsQueryHandler.HandleAsync(new GetSpraywallsQuery());
    }

    [HttpPost("{id}/problems")]
    [AllowAnonymous]
    public async Task<SpraywallProblemListDto> SearchSpraywallProblems(Guid id, [FromBody] SearchProblemsRequest? request)
    {
        var query = new SearchSpraywallProblemsQuery
        {
            SpraywallId = id,
            GradeMin = request?.GradeMin,
            GradeMax = request?.GradeMax,
            Name = request?.Name,
            Creator = request?.Creator,
            DateOrder = request?.DateOrder ?? "desc",
            Page = request?.Page ?? 1
        };

        return await _searchProblemsQueryHandler.HandleAsync(query);
    }

    [HttpPut("{id}/problem")]
    [Authorize(Roles = AuthorizationRoles.Editor)]
    public async Task<SpraywallProblemDto> CreateProblem(Guid id, [FromBody] CreateSpraywallProblemCommand command)
    {
        command.SpraywallId = id;
        await _createProblemCommandHandler.HandleAsync(command);

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        return await _getSpraywallProblemQueryHandler.HandleAsync(
            new GetSpraywallProblemQuery
            {
                Id = command.Id,
                CurrentUserId = currentUser?.Id,
                CurrentUserIsAdmin = isAdmin
            });
    }
}
