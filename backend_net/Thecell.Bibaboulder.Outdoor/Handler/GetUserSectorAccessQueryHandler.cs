using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetUserSectorAccessQueryHandler : IQueryHandler<GetUserSectorAccessQuery, UserSectorAccessDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUserSectorAccessQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<UserSectorAccessDto> HandleAsync(GetUserSectorAccessQuery query)
    {
        var access = await _dbContext.UserSectorAccesses
            .AsNoTracking()
            .SingleOrDefaultAsync(access => access.UserId == query.UserId && access.SectorId == query.SectorId)
            .ThrowIfNullAsync();

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can view user sector access.");
        }

        return new UserSectorAccessDto
        {
            UserId = access.UserId,
            SectorId = access.SectorId,
            AccessSourceType = access.AccessSourceType,
            ValidUntil = access.ValidUntil,
            Version = access.Version
        };
    }
}
