using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class DeleteBoulderGymCommandHandler : ICommandHandler<DeleteBoulderGymCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBoulderGymCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteBoulderGymCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

        var boulderGym = await _dbContext.BoulderGyms
            .Include(boulderGym => boulderGym.UriAliases)
            .Include(boulderGym => boulderGym.Spraywalls)
            .SingleOrDefaultAsync(gym => gym.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        if (boulderGym.UriAliases.Count != 0)
        {
            throw new InvalidOperationException($"Cannot delete boulder gym because it has associated uri aliases.");
        }

        if (boulderGym.Spraywalls.Count > 0)
        {
            foreach (var spraywall in boulderGym.Spraywalls)
            {
                spraywall.BoulderGymId = null;
            }

            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.RemoveEntityAndSaveChangesAsync(boulderGym, command.Version);
    }
}
