using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteOutdoorAreaCommandHandler : ICommandHandler<DeleteOutdoorAreaCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteOutdoorAreaCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteOutdoorAreaCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

        var outdoorArea = await _dbContext.OutdoorAreas
            .Include(outdoorArea => outdoorArea.UriAliases)
            .SingleOrDefaultAsync(area => area.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        if (outdoorArea.UriAliases.Count != 0)
        {
            throw new InvalidOperationException($"Cannot delete outdoor area because it has associated uri aliases.");
        }

        await _dbContext.RemoveEntityAndSaveChangesAsync(outdoorArea, command.Version);
    }
}
