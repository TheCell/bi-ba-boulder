using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Media.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MediasController : ControllerBase
{
    private readonly IQueryHandler<GetUriAliasQuery, UriAliasDto?> _getUriAliasQueryHandler;

    public MediasController(IQueryHandler<GetUriAliasQuery, UriAliasDto?> getUriAliasQueryHandler)
    {
        _getUriAliasQueryHandler = getUriAliasQueryHandler;
    }

    [HttpGet("{alias}/{type}")]
    [AllowAnonymous]
    public async Task<UriAliasDto?> GetUriAlias(string alias, UriType type)
    {
        return await _getUriAliasQueryHandler.HandleAsync(new GetUriAliasQuery { Alias = alias, Type = type });
    }
}
