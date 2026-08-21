using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize(Roles = AuthorizationRoles.Admin)]
[ApiController]
[Route("api/[controller]")]
public class UserSectorAccessesController : ControllerBase
{
    private readonly IQueryHandler<GetUserSectorAccessesQuery, ICollection<UserSectorAccessDto>> _getUserSectorAccessesQueryHandler;
    private readonly IQueryHandler<GetUserSectorAccessQuery, UserSectorAccessDto> _getUserSectorAccessQueryHandler;
    private readonly ICommandHandler<CreateUserSectorAccessCommand> _createUserSectorAccessCommandHandler;
    private readonly ICommandHandler<UpdateUserSectorAccessCommand> _updateUserSectorAccessCommandHandler;
    private readonly ICommandHandler<DeleteUserSectorAccessCommand> _deleteUserSectorAccessCommandHandler;

    public UserSectorAccessesController(
        IQueryHandler<GetUserSectorAccessesQuery, ICollection<UserSectorAccessDto>> getUserSectorAccessesQueryHandler,
        IQueryHandler<GetUserSectorAccessQuery, UserSectorAccessDto> getUserSectorAccessQueryHandler,
        ICommandHandler<CreateUserSectorAccessCommand> createUserSectorAccessCommandHandler,
        ICommandHandler<UpdateUserSectorAccessCommand> updateUserSectorAccessCommandHandler,
        ICommandHandler<DeleteUserSectorAccessCommand> deleteUserSectorAccessCommandHandler)
    {
        _getUserSectorAccessesQueryHandler = getUserSectorAccessesQueryHandler;
        _getUserSectorAccessQueryHandler = getUserSectorAccessQueryHandler;
        _createUserSectorAccessCommandHandler = createUserSectorAccessCommandHandler;
        _updateUserSectorAccessCommandHandler = updateUserSectorAccessCommandHandler;
        _deleteUserSectorAccessCommandHandler = deleteUserSectorAccessCommandHandler;
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ICollection<UserSectorAccessDto>> GetUserSectorAccesses()
    {
        return await _getUserSectorAccessesQueryHandler.HandleAsync(new GetUserSectorAccessesQuery());
    }

    [HttpGet("users/{userId}/sectors/{sectorId}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<UserSectorAccessDto> GetUserSectorAccess(Guid userId, Guid sectorId)
    {
        return await _getUserSectorAccessQueryHandler.HandleAsync(new GetUserSectorAccessQuery { UserId = userId, SectorId = sectorId });
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<UserSectorAccessDto> CreateUserSectorAccess([FromBody] CreateUserSectorAccessCommand command)
    {
        await _createUserSectorAccessCommandHandler.HandleAsync(command);
        return await _getUserSectorAccessQueryHandler.HandleAsync(new GetUserSectorAccessQuery { UserId = command.UserId, SectorId = command.SectorId });
    }

    [HttpPut("users/{userId}/sectors/{sectorId}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<UserSectorAccessDto> UpdateUserSectorAccess(Guid userId, Guid sectorId, [FromBody] UpdateUserSectorAccessCommand command)
    {
        command.UserId = userId;
        command.SectorId = sectorId;
        await _updateUserSectorAccessCommandHandler.HandleAsync(command);
        return await _getUserSectorAccessQueryHandler.HandleAsync(new GetUserSectorAccessQuery { UserId = userId, SectorId = sectorId });
    }

    [HttpDelete("users/{userId}/sectors/{sectorId}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task DeleteUserSectorAccess(Guid userId, Guid sectorId, [FromBody] DeleteUserSectorAccessCommand command)
    {
        command.UserId = userId;
        command.SectorId = sectorId;
        await _deleteUserSectorAccessCommandHandler.HandleAsync(command);
    }
}
