using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateOutdoorAreaCommandHandler : ICommandHandler<UpdateOutdoorAreaCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public UpdateOutdoorAreaCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(UpdateOutdoorAreaCommand command)
    {
        // todo add permission check
        var outdoorArea = await _dbContext.OutdoorAreas
            .Include(area => area.Sectors)
            .SingleOrDefaultAsync(area => area.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        var ids = command.SectorIds.Distinct().ToList();
        var sectors = await _dbContext.Sectors.Where(sector => ids.Contains(sector.Id)).ToListAsync();
        if (sectors.Count != ids.Count)
        {
            throw new ArgumentException("One or more sectors do not exist.");
        }

        outdoorArea.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);
        outdoorArea.Sectors = sectors;
        await _dbContext.UpdateEntityAndSaveChangesAsync(outdoorArea, command.Version);
    }
}
