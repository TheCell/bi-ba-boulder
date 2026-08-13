using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetLineQueryHandler : IQueryHandler<GetLineQuery, LineDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetLineQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<LineDto> HandleAsync(GetLineQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);
        var hasEditorRole = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Editor);

        var line = await _dbContext.Lines
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        if (!isAdmin)
        {
            var isSectorPublic = await _dbContext.Blocs
                .AsNoTracking()
                .Where(b => b.Id == line.BlocId)
                .Select(b => b.Sector!.IsPublic)
                .SingleOrDefaultAsync();

            if (!isSectorPublic)
            {
                if (currentUser is null)
                {
                    throw new AuthenticationException("Anonymous access requires the sector to be public.");
                }

                throw new AccessDeniedException("This sector is not public.");
            }
        }

        var lineDto = line.MapToLineDto();

        lineDto.Metadata.CanEdit = currentUser != null && currentUser.Id == line.CreatedUserId && hasEditorRole;
        lineDto.Metadata.CanDelete = currentUser != null && ((currentUser.Id == line.CreatedUserId && hasEditorRole) || isAdmin);
        return lineDto;
    }
}
