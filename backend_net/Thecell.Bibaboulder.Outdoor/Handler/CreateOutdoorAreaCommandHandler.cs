using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateOutdoorAreaCommandHandler : ICommandHandler<CreateOutdoorAreaCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateOutdoorAreaCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateOutdoorAreaCommand command)
    {
        // todo add permission check
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();
        var outdoorArea = new OutdoorArea { Id = Guid.CreateVersion7(), Name = command.Name, CreatedUserId = currentUser.Id };
        outdoorArea.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);
        outdoorArea.Sectors = await GetSectorsAsync(command.SectorIds);
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);
        command.Id = outdoorArea.Id;
    }

    private async Task<ICollection<Sector>> GetSectorsAsync(ICollection<Guid> sectorIds)
    {
        var ids = sectorIds.Distinct().ToList();
        var sectors = await _dbContext.Sectors.Where(sector => ids.Contains(sector.Id)).ToListAsync();
        if (sectors.Count != ids.Count)
        {
            throw new ArgumentException("One or more sectors do not exist.");
        }
        return sectors;
    }
}
