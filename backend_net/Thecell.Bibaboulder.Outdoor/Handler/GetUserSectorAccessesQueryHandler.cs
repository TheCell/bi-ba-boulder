using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetUserSectorAccessesQueryHandler : IQueryHandler<GetUserSectorAccessesQuery, ICollection<UserSectorAccessDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUserSectorAccessesQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ICollection<UserSectorAccessDto>> HandleAsync(GetUserSectorAccessesQuery query)
    {

        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can get user sector access.");
        }

        return await _dbContext.UserSectorAccesses
            .AsNoTracking()
            .OrderBy(access => access.UserId)
            .ThenBy(access => access.SectorId)
            .Select(access => new UserSectorAccessDto
            {
                UserId = access.UserId,
                SectorId = access.SectorId,
                AccessSourceType = access.AccessSourceType,
                ValidUntil = access.ValidUntil,
                Version = access.Version
            })
            .ToListAsync();
    }
}
