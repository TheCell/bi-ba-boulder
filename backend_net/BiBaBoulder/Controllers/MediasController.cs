using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
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
    private readonly IQueryHandler<GetUriAliasesQuery, ICollection<UriAliasAdministrationDto>> _getUriAliasesQueryHandler;
    private readonly IQueryHandler<GetUriAliasByIdQuery, UriAliasAdministrationDto> _getUriAliasByIdQueryHandler;
    private readonly ICommandHandler<CreateUriAliasCommand> _createUriAliasCommandHandler;
    private readonly ICommandHandler<UpdateUriAliasCommand> _updateUriAliasCommandHandler;
    private readonly ICommandHandler<DeleteUriAliasCommand> _deleteUriAliasCommandHandler;

    public MediasController(
        IQueryHandler<GetUriAliasQuery, UriAliasDto?> getUriAliasQueryHandler,
        IQueryHandler<GetUriAliasesQuery, ICollection<UriAliasAdministrationDto>> getUriAliasesQueryHandler,
        IQueryHandler<GetUriAliasByIdQuery, UriAliasAdministrationDto> getUriAliasByIdQueryHandler,
        ICommandHandler<CreateUriAliasCommand> createUriAliasCommandHandler,
        ICommandHandler<UpdateUriAliasCommand> updateUriAliasCommandHandler,
        ICommandHandler<DeleteUriAliasCommand> deleteUriAliasCommandHandler)
    {
        _getUriAliasQueryHandler = getUriAliasQueryHandler;
        _getUriAliasesQueryHandler = getUriAliasesQueryHandler;
        _getUriAliasByIdQueryHandler = getUriAliasByIdQueryHandler;
        _createUriAliasCommandHandler = createUriAliasCommandHandler;
        _updateUriAliasCommandHandler = updateUriAliasCommandHandler;
        _deleteUriAliasCommandHandler = deleteUriAliasCommandHandler;
    }

    [HttpGet("{alias}/{type}")]
    [AllowAnonymous]
    public async Task<UriAliasDto?> GetUriAlias(string alias, UriType type)
    {
        return await _getUriAliasQueryHandler.HandleAsync(new GetUriAliasQuery { Alias = alias, Type = type });
    }

    [HttpGet("uri-aliases")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<ICollection<UriAliasAdministrationDto>> GetUriAliases()
    {
        return await _getUriAliasesQueryHandler.HandleAsync(new GetUriAliasesQuery());
    }

    [HttpGet("uri-aliases/{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<UriAliasAdministrationDto> GetUriAliasById(Guid id)
    {
        return await _getUriAliasByIdQueryHandler.HandleAsync(new GetUriAliasByIdQuery { Id = id });
    }

    [HttpPost("uri-aliases")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<UriAliasAdministrationDto> CreateUriAlias([FromBody] CreateUriAliasCommand command)
    {
        await _createUriAliasCommandHandler.HandleAsync(command);
        return await _getUriAliasByIdQueryHandler.HandleAsync(new GetUriAliasByIdQuery { Id = command.Id });
    }

    [HttpPut("uri-aliases/{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<UriAliasAdministrationDto> UpdateUriAlias(Guid id, [FromBody] UpdateUriAliasCommand command)
    {
        command.Id = id;
        await _updateUriAliasCommandHandler.HandleAsync(command);
        return await _getUriAliasByIdQueryHandler.HandleAsync(new GetUriAliasByIdQuery { Id = id });
    }

    [HttpDelete("uri-aliases/{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task DeleteUriAlias(Guid id, [FromBody] DeleteUriAliasCommand command)
    {
        command.Id = id;
        await _deleteUriAliasCommandHandler.HandleAsync(command);
    }
}
