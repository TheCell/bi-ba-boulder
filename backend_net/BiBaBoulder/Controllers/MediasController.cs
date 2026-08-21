using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MediasController : ControllerBase
{
    private readonly IQueryHandler<GetBoulderGymQuery, BoulderGymDto> _getBoulderGymQueryHandler;

    public MediasController(
        IQueryHandler<GetBoulderGymQuery, BoulderGymDto> getBoulderGymQueryHandler
    )
    {
        _getBoulderGymQueryHandler = getBoulderGymQueryHandler;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<BoulderGymDto> GetBoulderGym(Guid id)
    {
        return await _getBoulderGymQueryHandler.HandleAsync(new GetBoulderGymQuery { Id = id });
    }
}
