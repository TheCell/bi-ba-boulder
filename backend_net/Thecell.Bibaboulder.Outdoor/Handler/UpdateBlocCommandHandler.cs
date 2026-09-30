using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateBlocCommandHandler : ICommandHandler<UpdateBlocCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public UpdateBlocCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(UpdateBlocCommand command)
    {
        var bloc = await _dbContext.Blocs
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.Sectors
            .AsNoTracking()
            .SingleOrDefaultAsync(sector => sector.Id == command.SectorId)
            .ThrowIfNullAsync(command.SectorId);

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
