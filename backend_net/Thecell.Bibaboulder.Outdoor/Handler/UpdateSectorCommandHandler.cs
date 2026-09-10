using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateSectorCommandHandler : ICommandHandler<UpdateSectorCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public UpdateSectorCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(UpdateSectorCommand command)
    {
        // todo add permission check
        var sector = await _dbContext.Sectors
            .Include(item => item.OutdoorAreas)
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        var ids = command.OutdoorAreaIds.Distinct().ToList();
        var areas = await _dbContext.OutdoorAreas.Where(area => ids.Contains(area.Id)).ToListAsync();
        if (areas.Count != ids.Count)
        {
            throw new ArgumentException("One or more outdoor areas do not exist.");
        }

        sector.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);
        sector.IsPublic = command.IsPublic;
        sector.Coordinates = command.Coordinates;
        sector.OutdoorAreas = areas;
        await _dbContext.UpdateEntityAndSaveChangesAsync(sector, command.Version);
    }
}
