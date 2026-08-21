using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Queries;
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

    public OutdoorAreasController(
        IQueryHandler<GetOutdoorAreaQuery, OutdoorAreaDto> getOutdoorAreaQueryHandler,
        IQueryHandler<GetOutdoorAreasQuery, ICollection<OutdoorAreaDto>> getOutdoorAreasQueryHandler)
    {
        _getOutdoorAreaQueryHandler = getOutdoorAreaQueryHandler;
        _getOutdoorAreasQueryHandler = getOutdoorAreasQueryHandler;
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
}
