using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Access;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateUserSectorAccessCommandHandler : ICommandHandler<CreateUserSectorAccessCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateUserSectorAccessCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateUserSectorAccessCommand command)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(user => user.Id == command.UserId)
            .ThrowIfNullAsync(command.UserId);

        var sector = await _dbContext.Sectors.SingleOrDefaultAsync(sector => sector.Id == command.SectorId)
            .ThrowIfNullAsync(command.SectorId);

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can create user sector access.");
        }

        var accessExists = await _dbContext.UserSectorAccesses
            .AnyAsync(access => access.UserId == command.UserId && access.SectorId == command.SectorId);

        if (accessExists)
        {
            throw new InvalidOperationException("Access already exists for this user and sector.");
        }

        var access = new UserSectorAccess
        {
            UserId = user.Id,
            User = user,
            SectorId = sector.Id,
            Sector = sector,
            AccessSourceType = command.AccessSourceType,
            ValidUntil = command.ValidUntil
        };

        await _dbContext.InsertEntityAndSaveChangesAsync(access);
    }
}
