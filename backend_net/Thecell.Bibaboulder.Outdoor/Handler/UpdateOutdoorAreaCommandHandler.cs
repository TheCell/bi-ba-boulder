using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateOutdoorAreaCommandHandler : ICommandHandler<UpdateOutdoorAreaCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOutdoorAreaCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateOutdoorAreaCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

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
