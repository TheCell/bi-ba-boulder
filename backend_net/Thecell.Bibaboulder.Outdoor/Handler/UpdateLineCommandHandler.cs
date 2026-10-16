using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateLineCommandHandler : ICommandHandler<UpdateLineCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLineCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateLineCommand command)
    {
        if (command.Data.Positions.Count < 3)
        {
            throw new ArgumentException("A line must have at least 3 positions.");
        }

        var line = await _dbContext.Lines
            .SingleOrDefaultAsync(l => l.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);
        var hasEditorRole = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Editor);

        if (currentUser is null || (!isAdmin && !(currentUser.Id == line.CreatedUserId && hasEditorRole)))
        {
            throw new AccessDeniedException("Only the creator or an admin can delete this line");
        }

        line.Identifier = command.Identifier.Trim();
        line.Name = command.Name?.Trim();
        line.Description = command.Description;
        line.FontGrade = command.FontGrade;
        line.Data = command.Data;

        await _dbContext.UpdateEntityAndSaveChangesAsync(line, command.Version);
    }
}
