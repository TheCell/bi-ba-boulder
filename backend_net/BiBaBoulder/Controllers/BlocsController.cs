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

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BlocsController : ControllerBase
{
    private readonly IQueryHandler<GetBlocsBySectorIdQuery, ICollection<BlocDto>> _getBlocsBySectorIdQueryHandler;
    private readonly IQueryHandler<GetBlocQuery, BlocDto> _getBlocQueryHandler;
    private readonly ICommandHandler<CreateBlocCommand> _createBlocCommandHandler;
    private readonly ICommandHandler<UpdateBlocCommand> _updateBlocCommandHandler;
    private readonly ICommandHandler<DeleteBlocCommand> _deleteBlocCommandHandler;

    public BlocsController(
        IQueryHandler<GetBlocsBySectorIdQuery, ICollection<BlocDto>> getBlocsBySectorIdQueryHandler,
        IQueryHandler<GetBlocQuery, BlocDto> getBlocQueryHandler,
        ICommandHandler<CreateBlocCommand> createBlocCommandHandler,
        ICommandHandler<UpdateBlocCommand> updateBlocCommandHandler,
        ICommandHandler<DeleteBlocCommand> deleteBlocCommandHandler)
    {
        _getBlocsBySectorIdQueryHandler = getBlocsBySectorIdQueryHandler;
        _getBlocQueryHandler = getBlocQueryHandler;
        _createBlocCommandHandler = createBlocCommandHandler;
        _updateBlocCommandHandler = updateBlocCommandHandler;
        _deleteBlocCommandHandler = deleteBlocCommandHandler;
    }

    [HttpGet("by-sector/{sectorId}")]
    [AllowAnonymous]
    public async Task<ICollection<BlocDto>> GetBlocsBySectorId(Guid sectorId)
    {
        return await _getBlocsBySectorIdQueryHandler.HandleAsync(
            new GetBlocsBySectorIdQuery { SectorId = sectorId });
    }

    [HttpGet("without-sector")]
    [AllowAnonymous]
    public async Task<ICollection<BlocDto>> GetBlocsWithoutSector()
    {
        return await _getBlocsBySectorIdQueryHandler.HandleAsync(
            new GetBlocsBySectorIdQuery { SectorId = null });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<BlocDto> GetBloc(Guid id)
    {
        return await _getBlocQueryHandler.HandleAsync(new GetBlocQuery { Id = id });
    }

    [HttpPost]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<BlocDto> CreateBloc(CreateBlocCommand command)
    {
        await _createBlocCommandHandler.HandleAsync(command);
        return await _getBlocQueryHandler.HandleAsync(new GetBlocQuery { Id = command.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task<BlocDto> UpdateBloc(Guid id, UpdateBlocCommand command)
    {
        command.Id = id;
        await _updateBlocCommandHandler.HandleAsync(command);
        return await _getBlocQueryHandler.HandleAsync(new GetBlocQuery { Id = id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{AuthorizationRoles.ContentAdmin},{AuthorizationRoles.Admin}")]
    public async Task DeleteBloc(Guid id, DeleteBlocCommand command)
    {
        command.Id = id;
        await _deleteBlocCommandHandler.HandleAsync(command);
    }
}
