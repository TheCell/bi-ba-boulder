using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateBlocCommandHandler : ICommandHandler<UpdateBlocCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBlocCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateBlocCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var bloc = await _dbContext.Blocs
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        if (!currentUser.IsInRole(UserRole.Admin) && currentUser.IsInRole(UserRole.ContentAdmin) && currentUser.Id != bloc.CreatedUserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this bloc.");
        }

        bloc.SectorId = command.SectorId;
        bloc.Name = command.Name.Trim();
        bloc.Description = command.Description;
        bloc.Coordinates = command.Coordinates;
        bloc.BlocLowRes = command.BlocLowRes;
        bloc.BlocMedRes = command.BlocMedRes;
        bloc.BlocHighRes = command.BlocHighRes;
        bloc.PreviewImageUri = command.PreviewImageUri;
        await _dbContext.UpdateEntityAndSaveChangesAsync(bloc, command.Version);
    }
}
