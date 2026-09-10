using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class UpdateBoulderGymCommandHandler : ICommandHandler<UpdateBoulderGymCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBoulderGymCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateBoulderGymCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

        var boulderGym = await _dbContext.BoulderGyms
            .SingleOrDefaultAsync(gym => gym.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        boulderGym.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);
        await _dbContext.UpdateEntityAndSaveChangesAsync(boulderGym, command.Version);
    }
}
