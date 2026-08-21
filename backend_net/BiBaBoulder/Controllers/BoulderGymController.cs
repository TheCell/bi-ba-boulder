using System;
using System.Collections.Generic;
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
public class BoulderGymController : ControllerBase
{
    private readonly IQueryHandler<GetBoulderGymQuery, BoulderGymDto> _getBoulderGymQueryHandler;
    private readonly IQueryHandler<GetBoulderGymsQuery, ICollection<BoulderGymDto>> _getBoulderGymsQueryHandler;

    public BoulderGymController(
        IQueryHandler<GetBoulderGymQuery, BoulderGymDto> getBoulderGymQueryHandler,
        IQueryHandler<GetBoulderGymsQuery, ICollection<BoulderGymDto>> getBoulderGymsQueryHandler)
    {
        _getBoulderGymQueryHandler = getBoulderGymQueryHandler;
        _getBoulderGymsQueryHandler = getBoulderGymsQueryHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ICollection<BoulderGymDto>> GetBoulderGyms()
    {
        return await _getBoulderGymsQueryHandler.HandleAsync(new GetBoulderGymsQuery());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<BoulderGymDto> GetBoulderGym(Guid id)
    {
        return await _getBoulderGymQueryHandler.HandleAsync(new GetBoulderGymQuery { Id = id });
    }
}
