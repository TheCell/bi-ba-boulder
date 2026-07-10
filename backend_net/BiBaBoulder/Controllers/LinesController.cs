using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public LinesController(
        IQueryHandler<GetLinesByBlocIdQuery, ICollection<LineDto>> getLinesByBlocIdQueryHandler)
    {
        _getLinesByBlocIdQueryHandler = getLinesByBlocIdQueryHandler;
    }

    [HttpGet("by-bloc/{blocId}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ICollection<LineDto>> GetLinesByBlocId(Guid blocId)
    {
        return await _getLinesByBlocIdQueryHandler.HandleAsync(
            new GetLinesByBlocIdQuery { BlocId = blocId });
    }
}
