using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateSectorCommandHandler : ICommandHandler<CreateSectorCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSectorCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateSectorCommand command)
    {
        // todo add permission check
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var sector = new Sector
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Description = command.Description,
            ImportantInfo = command.ImportantInfo,
            IsPublic = command.IsPublic,
            Coordinates = command.Coordinates,
            PreviewImageUri = command.PreviewImageUri,
            CreatedUserId = currentUser.Id
        };

        sector.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);
        var outdoorAreaIds = command.OutdoorAreaIds.Distinct().ToList();
        var outdoorAreas = await _dbContext.OutdoorAreas.Where(area => outdoorAreaIds.Contains(area.Id)).ToListAsync();
        if (outdoorAreas.Count != outdoorAreaIds.Count)
        {
            throw new ArgumentException("One or more outdoor areas do not exist.");
        }
        sector.OutdoorAreas = outdoorAreas;
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);
        command.Id = sector.Id;
    }
}
