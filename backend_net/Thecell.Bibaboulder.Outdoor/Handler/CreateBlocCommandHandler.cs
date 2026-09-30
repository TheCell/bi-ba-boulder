using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateBlocCommandHandler : ICommandHandler<CreateBlocCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public CreateBlocCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(CreateBlocCommand command)
    {
        await _dbContext.Sectors
            .AsNoTracking()
            .SingleOrDefaultAsync(sector => sector.Id == command.SectorId)
            .ThrowIfNullAsync(command.SectorId);

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
