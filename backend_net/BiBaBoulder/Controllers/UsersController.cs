using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Spraywall.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IQueryHandler<GetCurrentUserQuery, UserDto> _getCurrentUserQueryHandler;
    private readonly IQueryHandler<GetUserByIdQuery, UserDto> _getUserByIdQueryHandler;

    public UsersController(
        IQueryHandler<GetCurrentUserQuery, UserDto> getCurrentUserQueryHandler,
        IQueryHandler<GetUserByIdQuery, UserDto> getUserByIdQueryHandler)
    {
        _getCurrentUserQueryHandler = getCurrentUserQueryHandler;
        _getUserByIdQueryHandler = getUserByIdQueryHandler;
    }

    [HttpGet("me")]
    [Authorize(Roles = AuthorizationRoles.User)]
    public async Task<UserDto> GetSelf()
    {
        return await _getCurrentUserQueryHandler.HandleAsync(
            new GetCurrentUserQuery { });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<UserDto> GetUserById(Guid id)
    {
        return await _getUserByIdQueryHandler.HandleAsync(
            new GetUserByIdQuery { Id = id });
    }
}
