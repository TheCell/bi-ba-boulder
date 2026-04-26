using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BlocsController : ControllerBase
{
    private readonly IQueryHandler<GetBlocsBySectorIdQuery, ICollection<BlocDto>> _getBlocsBySectorIdQueryHandler;
    private readonly IQueryHandler<GetBlocQuery, BlocDto> _getBlocQueryHandler;

    public BlocsController(
        IQueryHandler<GetBlocsBySectorIdQuery, ICollection<BlocDto>> getBlocsBySectorIdQueryHandler,
        IQueryHandler<GetBlocQuery, BlocDto> getBlocQueryHandler)
    {
        _getBlocsBySectorIdQueryHandler = getBlocsBySectorIdQueryHandler;
        _getBlocQueryHandler = getBlocQueryHandler;
    }

    [HttpGet("by-sector/{sectorId}")]
    [AllowAnonymous]
    public async Task<ICollection<BlocDto>> GetBlocsBySectorId(Guid sectorId)
    {
        return await _getBlocsBySectorIdQueryHandler.HandleAsync(
            new GetBlocsBySectorIdQuery { SectorId = sectorId });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<BlocDto> GetBloc(Guid id)
    {
        return await _getBlocQueryHandler.HandleAsync(new GetBlocQuery { Id = id });
    }
}
