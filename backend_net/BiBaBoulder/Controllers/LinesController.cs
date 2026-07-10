using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LinesController : ControllerBase
{
    private readonly IQueryHandler<GetLinesByBlocIdQuery, ICollection<LineDto>> _getLinesByBlocIdQueryHandler;
    private readonly IQueryHandler<GetLineQuery, LineDto> _getLineQueryHandler;
    private readonly ICommandHandler<CreateLineCommand> _createLineCommandHandler;
    private readonly ICommandHandler<UpdateLineCommand> _updateLineCommandHandler;
    private readonly ICommandHandler<DeleteLineCommand> _deleteLineCommandHandler;

    public LinesController(
        IQueryHandler<GetLinesByBlocIdQuery, ICollection<LineDto>> getLinesByBlocIdQueryHandler,
        IQueryHandler<GetLineQuery, LineDto> getLineQueryHandler,
        ICommandHandler<CreateLineCommand> createLineCommandHandler,
        ICommandHandler<UpdateLineCommand> updateLineCommandHandler,
        ICommandHandler<DeleteLineCommand> deleteLineCommandHandler)
    {
        _getLinesByBlocIdQueryHandler = getLinesByBlocIdQueryHandler;
        _getLineQueryHandler = getLineQueryHandler;
        _createLineCommandHandler = createLineCommandHandler;
        _updateLineCommandHandler = updateLineCommandHandler;
        _deleteLineCommandHandler = deleteLineCommandHandler;
    }

    [HttpGet("by-bloc/{blocId}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ICollection<LineDto>> GetLinesByBlocId(Guid blocId)
    {
        return await _getLinesByBlocIdQueryHandler.HandleAsync(
            new GetLinesByBlocIdQuery { BlocId = blocId });
    }

    [HttpPost("for-bloc/{blocId}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<LineDto> CreateLineForBloc(Guid blocId, [FromBody] CreateLineCommand command)
    {
        command.BlocId = blocId;
        await _createLineCommandHandler.HandleAsync(command);
        return await _getLineQueryHandler.HandleAsync(new GetLineQuery { Id = command.Id });
    }

    [HttpPost("{id}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<LineDto> UpdateLine(Guid id, [FromBody] UpdateLineCommand command)
    {
        command.Id = id;
        await _updateLineCommandHandler.HandleAsync(command);
        return await _getLineQueryHandler.HandleAsync(new GetLineQuery { Id = id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task DeleteLine(Guid id, [FromBody] DeleteLineCommand command)
    {
        command.Id = id;
        await _deleteLineCommandHandler.HandleAsync(command);
    }
}
