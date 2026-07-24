using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteLineCommandHandler : ICommandHandler<DeleteLineCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLineCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteLineCommand command)
    {
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

        await _dbContext.RemoveEntityAndSaveChangesAsync(line, line.Version);
    }
}
