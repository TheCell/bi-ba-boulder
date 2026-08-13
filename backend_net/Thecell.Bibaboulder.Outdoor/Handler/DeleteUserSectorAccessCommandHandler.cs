using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteUserSectorAccessCommandHandler : ICommandHandler<DeleteUserSectorAccessCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserSectorAccessCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteUserSectorAccessCommand command)
    {
        var access = await _dbContext.UserSectorAccesses
            .SingleOrDefaultAsync(access => access.UserId == command.UserId && access.SectorId == command.SectorId)
            .ThrowIfNullAsync();

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can delete user sector access.");
        }

        await _dbContext.RemoveEntityAndSaveChangesAsync(access, command.Version);
    }
}
