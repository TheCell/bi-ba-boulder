using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OutdoorAreasController : ControllerBase
{
    private readonly IQueryHandler<GetOutdoorAreaQuery, OutdoorAreaDto> _getOutdoorAreaQueryHandler;
    private readonly IQueryHandler<GetOutdoorAreasQuery, ICollection<OutdoorAreaDto>> _getOutdoorAreasQueryHandler;
    private readonly ICommandHandler<CreateOutdoorAreaCommand> _createOutdoorAreaCommandHandler;
    private readonly ICommandHandler<UpdateOutdoorAreaCommand> _updateOutdoorAreaCommandHandler;
    private readonly ICommandHandler<DeleteOutdoorAreaCommand> _deleteOutdoorAreaCommandHandler;

    public OutdoorAreasController(
        IQueryHandler<GetOutdoorAreaQuery, OutdoorAreaDto> getOutdoorAreaQueryHandler,
        IQueryHandler<GetOutdoorAreasQuery, ICollection<OutdoorAreaDto>> getOutdoorAreasQueryHandler,
        ICommandHandler<CreateOutdoorAreaCommand> createOutdoorAreaCommandHandler,
        ICommandHandler<UpdateOutdoorAreaCommand> updateOutdoorAreaCommandHandler,
        ICommandHandler<DeleteOutdoorAreaCommand> deleteOutdoorAreaCommandHandler)
    {
        _getOutdoorAreaQueryHandler = getOutdoorAreaQueryHandler;
        _getOutdoorAreasQueryHandler = getOutdoorAreasQueryHandler;
        _createOutdoorAreaCommandHandler = createOutdoorAreaCommandHandler;
        _updateOutdoorAreaCommandHandler = updateOutdoorAreaCommandHandler;
        _deleteOutdoorAreaCommandHandler = deleteOutdoorAreaCommandHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ICollection<OutdoorAreaDto>> GetOutdoorAreas()
    {
        return await _getOutdoorAreasQueryHandler.HandleAsync(new GetOutdoorAreasQuery());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<OutdoorAreaDto> GetOutdoorArea(Guid id)
    {
        return await _getOutdoorAreaQueryHandler.HandleAsync(new GetOutdoorAreaQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<OutdoorAreaDto> CreateOutdoorArea(CreateOutdoorAreaCommand command)
    {
        await _createOutdoorAreaCommandHandler.HandleAsync(command);
        return await _getOutdoorAreaQueryHandler.HandleAsync(new GetOutdoorAreaQuery { Id = command.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<OutdoorAreaDto> UpdateOutdoorArea(Guid id, UpdateOutdoorAreaCommand command)
    {
        command.Id = id;
        await _updateOutdoorAreaCommandHandler.HandleAsync(command);
        return await _getOutdoorAreaQueryHandler.HandleAsync(new GetOutdoorAreaQuery { Id = id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task DeleteOutdoorArea(Guid id, DeleteOutdoorAreaCommand command)
    {
        command.Id = id;
        await _deleteOutdoorAreaCommandHandler.HandleAsync(command);
    }
}
