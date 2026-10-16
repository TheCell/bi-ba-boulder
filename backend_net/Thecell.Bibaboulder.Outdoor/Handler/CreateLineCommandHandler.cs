using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateLineCommandHandler : ICommandHandler<CreateLineCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateLineCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateLineCommand command)
    {
        if (command.Data.Positions.Count < 3)
        {
            throw new ArgumentException("A line must have at least 3 positions.");
        }

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);
        var hasEditorRole = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Editor);

        if (currentUser is null || (!isAdmin && !hasEditorRole))
        {
            throw new AccessDeniedException("Only the creator or an admin can delete this line");
        }

        await _dbContext.Blocs
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == command.BlocId)
            .ThrowIfNullAsync(command.BlocId);

        var line = new Line
        {
            Id = Guid.CreateVersion7(),
            BlocId = command.BlocId,
            Identifier = command.Identifier.Trim(),
            Name = command.Name?.Trim(),
            Description = command.Description,
            FontGrade = command.FontGrade,
            Data = command.Data
        };

        await _dbContext.InsertEntityAndSaveChangesAsync(line);
        command.Id = line.Id;
    }
}
