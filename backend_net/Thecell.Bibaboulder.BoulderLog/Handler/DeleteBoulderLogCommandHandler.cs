using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class DeleteBoulderLogCommandHandler : ICommandHandler<DeleteBoulderLogCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBoulderLogCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteBoulderLogCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var boulderLog = await _dbContext.BoulderLogs
            .SingleOrDefaultAsync(b => b.Id == command.Id);

        NotFoundException.ThrowIfNull(boulderLog, nameof(Model.Model.BoulderLog), command.Id);

        if (boulderLog.UserId != currentUser.Id)
        {
            throw new UnauthorizedAccessException("User does not own this boulder log.");
        }

        await _dbContext.RemoveEntityAndSaveChangesAsync(boulderLog, command.Version);
    }
}
