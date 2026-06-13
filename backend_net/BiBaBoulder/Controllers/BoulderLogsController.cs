using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

[Authorize(Roles = AuthorizationRoles.User)]
[ApiController]
[Route("api/[controller]")]
public class BoulderLogsController : ControllerBase
{
    private readonly IQueryHandler<GetBoulderLogQuery, BoulderLogDto?> _getBoulderLogQueryHandler;
    private readonly IQueryHandler<GetBoulderLogsQuery, ICollection<BoulderLogDto>> _getBoulderLogsQueryHandler;
    private readonly ICommandHandler<CreateBoulderLogCommand> _createBoulderLogCommandHandler;
    private readonly ICommandHandler<UpdateBoulderLogCommand> _updateBoulderLogCommandHandler;
    private readonly ICommandHandler<DeleteBoulderLogCommand> _deleteBoulderLogCommandHandler;
    private readonly ICurrentUserService _currentUserService;

    public BoulderLogsController(
        IQueryHandler<GetBoulderLogQuery, BoulderLogDto?> getBoulderLogQueryHandler,
        IQueryHandler<GetBoulderLogsQuery, ICollection<BoulderLogDto>> getBoulderLogsQueryHandler,
        ICommandHandler<CreateBoulderLogCommand> createBoulderLogCommandHandler,
        ICommandHandler<UpdateBoulderLogCommand> updateBoulderLogCommandHandler,
        ICommandHandler<DeleteBoulderLogCommand> deleteBoulderLogCommandHandler,
        ICurrentUserService currentUserService)
    {
        _getBoulderLogQueryHandler = getBoulderLogQueryHandler;
        _getBoulderLogsQueryHandler = getBoulderLogsQueryHandler;
        _createBoulderLogCommandHandler = createBoulderLogCommandHandler;
        _updateBoulderLogCommandHandler = updateBoulderLogCommandHandler;
        _deleteBoulderLogCommandHandler = deleteBoulderLogCommandHandler;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ICollection<BoulderLogDto>> GetBoulderLogs()
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();
        return await _getBoulderLogsQueryHandler.HandleAsync(new GetBoulderLogsQuery { UserId = currentUser.Id });
    }

    [HttpGet("{id}")]
    public async Task<BoulderLogDto?> GetBoulderLog(Guid id)
    {
        return await _getBoulderLogQueryHandler.HandleAsync(new GetBoulderLogQuery { Id = id });
    }

    [HttpPut]
    public async Task<BoulderLogDto> CreateBoulderLog([FromBody] CreateBoulderLogCommand command)
    {
        await _createBoulderLogCommandHandler.HandleAsync(command);
        return await _getBoulderLogQueryHandler.HandleAsync(new GetBoulderLogQuery { Id = command.Id }) ?? throw new InvalidOperationException("Failed to create boulder log.");
    }

    [HttpPost("{id}")]
    public async Task<BoulderLogDto?> UpdateBoulderLog(Guid id, [FromBody] UpdateBoulderLogCommand command)
    {
        command.Id = id;
        await _updateBoulderLogCommandHandler.HandleAsync(command);
        return await _getBoulderLogQueryHandler.HandleAsync(new GetBoulderLogQuery { Id = id });
    }

    [HttpDelete("{id}")]
    public async Task DeleteBoulderLog(Guid id, [FromBody] DeleteBoulderLogCommand command)
    {
        command.Id = id;
        await _deleteBoulderLogCommandHandler.HandleAsync(command);
    }
}
