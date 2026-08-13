using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateUserSectorAccessCommandHandler : ICommandHandler<UpdateUserSectorAccessCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserSectorAccessCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateUserSectorAccessCommand command)
    {
        var access = await _dbContext.UserSectorAccesses
            .SingleOrDefaultAsync(access => access.UserId == command.UserId && access.SectorId == command.SectorId)
            .ThrowIfNullAsync();

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can update user sector access.");
        }

        access.AccessSourceType = command.AccessSourceType;
        access.ValidUntil = command.ValidUntil;

        await _dbContext.UpdateEntityAndSaveChangesAsync(access, command.Version);
    }
}
