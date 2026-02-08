using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectorsController : ControllerBase
{
    private readonly IQueryHandler<GetSectorQuery, SectorDto> _getSectorQueryHandler;
    private readonly ICommandHandler<CreateSectorCommand> _createSectorCommandHandler;

    public SectorsController(
        IQueryHandler<GetSectorQuery, SectorDto> getSectorQueryHandler,
        ICommandHandler<CreateSectorCommand> createSectorCommandHandler)
    {
        _getSectorQueryHandler = getSectorQueryHandler;
        _createSectorCommandHandler = createSectorCommandHandler;
    }

    [HttpGet("{id}")]
    public async Task<SectorDto> GetSector(Guid id)
    {
        var query = new GetSectorQuery { Id = id };
        return await _getSectorQueryHandler.HandleAsync(query);
    }

    [HttpPost]
    public async Task<SectorDto> CreateSector(CreateSectorCommand command)
    {
        await _createSectorCommandHandler.HandleAsync(command);
        return await _getSectorQueryHandler.HandleAsync(new GetSectorQuery { Id = command.Id });
    }
}
