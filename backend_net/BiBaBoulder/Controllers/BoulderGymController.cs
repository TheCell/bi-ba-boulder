using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BoulderGymController : ControllerBase
{
    private readonly IQueryHandler<GetBoulderGymQuery, BoulderGymDto> _getBoulderGymQueryHandler;
    private readonly IQueryHandler<GetBoulderGymsQuery, ICollection<BoulderGymDto>> _getBoulderGymsQueryHandler;
    private readonly ICommandHandler<CreateBoulderGymCommand> _createBoulderGymCommandHandler;
    private readonly ICommandHandler<UpdateBoulderGymCommand> _updateBoulderGymCommandHandler;
    private readonly ICommandHandler<DeleteBoulderGymCommand> _deleteBoulderGymCommandHandler;

    public BoulderGymController(
        IQueryHandler<GetBoulderGymQuery, BoulderGymDto> getBoulderGymQueryHandler,
        IQueryHandler<GetBoulderGymsQuery, ICollection<BoulderGymDto>> getBoulderGymsQueryHandler,
        ICommandHandler<CreateBoulderGymCommand> createBoulderGymCommandHandler,
        ICommandHandler<UpdateBoulderGymCommand> updateBoulderGymCommandHandler,
        ICommandHandler<DeleteBoulderGymCommand> deleteBoulderGymCommandHandler)
    {
        _getBoulderGymQueryHandler = getBoulderGymQueryHandler;
        _getBoulderGymsQueryHandler = getBoulderGymsQueryHandler;
        _createBoulderGymCommandHandler = createBoulderGymCommandHandler;
        _updateBoulderGymCommandHandler = updateBoulderGymCommandHandler;
        _deleteBoulderGymCommandHandler = deleteBoulderGymCommandHandler;
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

    [HttpPost]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<BoulderGymDto> CreateBoulderGym(CreateBoulderGymCommand command)
    {
        await _createBoulderGymCommandHandler.HandleAsync(command);
        return await _getBoulderGymQueryHandler.HandleAsync(new GetBoulderGymQuery { Id = command.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<BoulderGymDto> UpdateBoulderGym(Guid id, UpdateBoulderGymCommand command)
    {
        command.Id = id;
        await _updateBoulderGymCommandHandler.HandleAsync(command);
        return await _getBoulderGymQueryHandler.HandleAsync(new GetBoulderGymQuery { Id = id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task DeleteBoulderGym(Guid id, DeleteBoulderGymCommand command)
    {
        command.Id = id;
        await _deleteBoulderGymCommandHandler.HandleAsync(command);
    }
}
