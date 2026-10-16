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

public class UpdateSectorCommandHandler : ICommandHandler<UpdateSectorCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSectorCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateSectorCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var sector = await _dbContext.Sectors
            .Include(item => item.OutdoorAreas)
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        if (!currentUser.IsInRole(UserRole.Admin) && currentUser.IsInRole(UserRole.ContentAdmin) && currentUser.Id != sector.CreatedUserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this sector.");
        }

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
