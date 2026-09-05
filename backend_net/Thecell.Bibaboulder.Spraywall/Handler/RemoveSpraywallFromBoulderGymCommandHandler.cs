using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class RemoveSpraywallFromBoulderGymCommandHandler : ICommandHandler<RemoveSpraywallFromBoulderGymCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RemoveSpraywallFromBoulderGymCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(RemoveSpraywallFromBoulderGymCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

        var boulderGym = await _dbContext.BoulderGyms
            .SingleOrDefaultAsync(gym => gym.Id == command.BoulderGymId)
            .ThrowIfNullAsync(command.BoulderGymId);

        var spraywall = await _dbContext.Spraywalls
            .SingleOrDefaultAsync(s => s.Id == command.SpraywallId)
            .ThrowIfNullAsync(command.SpraywallId);

        if (spraywall.BoulderGymId == boulderGym.Id)
        {
            spraywall.BoulderGymId = null;
            await _dbContext.SaveChangesAsync();
        }
    }
}
