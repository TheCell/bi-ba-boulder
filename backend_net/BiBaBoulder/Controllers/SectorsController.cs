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
public class SectorsController : ControllerBase
{
    private readonly IQueryHandler<GetSectorQuery, SectorDto> _getSectorQueryHandler;
    private readonly IQueryHandler<GetSectorsQuery, ICollection<SectorDto>> _getSectorsQueryHandler;
    private readonly ICommandHandler<CreateSectorCommand> _createSectorCommandHandler;
    private readonly ICommandHandler<UpdateSectorCommand> _updateSectorCommandHandler;
    private readonly ICommandHandler<DeleteSectorCommand> _deleteSectorCommandHandler;

    public SectorsController(
        IQueryHandler<GetSectorQuery, SectorDto> getSectorQueryHandler,
        IQueryHandler<GetSectorsQuery, ICollection<SectorDto>> getSectorsQueryHandler,
        ICommandHandler<CreateSectorCommand> createSectorCommandHandler,
        ICommandHandler<UpdateSectorCommand> updateSectorCommandHandler,
        ICommandHandler<DeleteSectorCommand> deleteSectorCommandHandler)
    {
        _getSectorQueryHandler = getSectorQueryHandler;
        _getSectorsQueryHandler = getSectorsQueryHandler;
        _createSectorCommandHandler = createSectorCommandHandler;
        _updateSectorCommandHandler = updateSectorCommandHandler;
        _deleteSectorCommandHandler = deleteSectorCommandHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ICollection<SectorDto>> GetSectors()
    {
        return await _getSectorsQueryHandler.HandleAsync(new GetSectorsQuery());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<SectorDto> GetSector(Guid id)
    {
        var query = new GetSectorQuery { Id = id };
        return await _getSectorQueryHandler.HandleAsync(query);
    }

    [HttpPost]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<SectorDto> CreateSector(CreateSectorCommand command)
    {
        await _createSectorCommandHandler.HandleAsync(command);
        return await _getSectorQueryHandler.HandleAsync(new GetSectorQuery { Id = command.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<SectorDto> UpdateSector(Guid id, UpdateSectorCommand command)
    {
        command.Id = id;
        await _updateSectorCommandHandler.HandleAsync(command);
        return await _getSectorQueryHandler.HandleAsync(new GetSectorQuery { Id = id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task DeleteSector(Guid id, DeleteSectorCommand command)
    {
        command.Id = id;
        await _deleteSectorCommandHandler.HandleAsync(command);
    }
}
