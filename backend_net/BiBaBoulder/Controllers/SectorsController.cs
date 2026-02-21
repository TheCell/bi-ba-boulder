using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.BiBaBoulder.Extensions;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
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
    private readonly IBiBaBoulderDbContext _dbContext;

    public SectorsController(
        IBiBaBoulderDbContext dbContext,
        IQueryHandler<GetSectorQuery, SectorDto> getSectorQueryHandler,
        IQueryHandler<GetSectorsQuery, ICollection<SectorDto>> getSectorsQueryHandler,
        ICommandHandler<CreateSectorCommand> createSectorCommandHandler)
    {
        _dbContext = dbContext;
        _getSectorQueryHandler = getSectorQueryHandler;
        _getSectorsQueryHandler = getSectorsQueryHandler;
        _createSectorCommandHandler = createSectorCommandHandler;
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
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<SectorDto> CreateSector(CreateSectorCommand command)
    {
        await _createSectorCommandHandler.HandleAsync(command);
        return await _getSectorQueryHandler.HandleAsync(new GetSectorQuery { Id = command.Id });
    }
}
