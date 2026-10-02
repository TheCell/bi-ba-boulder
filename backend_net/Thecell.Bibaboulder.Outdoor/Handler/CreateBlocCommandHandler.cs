using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateBlocCommandHandler : ICommandHandler<CreateBlocCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateBlocCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateBlocCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User is not authorized to create a bloc.");
        }

        var bloc = new Bloc
        {
            Id = Guid.CreateVersion7(),
            SectorId = command.SectorId,
            Name = command.Name.Trim(),
            Description = command.Description,
            Coordinates = command.Coordinates,
            BlocLowRes = command.BlocLowRes,
            BlocMedRes = command.BlocMedRes,
            BlocHighRes = command.BlocHighRes,
            PreviewImageUri = command.PreviewImageUri
        };

        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);
        command.Id = bloc.Id;
    }
}
