using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetLinesByBlocIdQueryHandler : IQueryHandler<GetLinesByBlocIdQuery, ICollection<LineDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetLinesByBlocIdQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ICollection<LineDto>> HandleAsync(GetLinesByBlocIdQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);
        var hasEditorRole = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Editor);

        var lines = await _dbContext.Lines
            .AsNoTracking()
            .Where(l => l.BlocId == query.BlocId)
            .OrderBy(l => l.Identifier)
            .ToListAsync();

        var lineDtos = lines.Select(l => l.MapToLineDto()).ToList();

        foreach (var lineDto in lineDtos)
        {
            var line = lines.Single(l => l.Id == lineDto.Id);
            lineDto.Metadata.CanEdit = currentUser != null && currentUser.Id == line.CreatedUserId && hasEditorRole;
            lineDto.Metadata.CanDelete = currentUser != null && ((currentUser.Id == line.CreatedUserId && hasEditorRole) || isAdmin);
        }

        return lineDtos;
    }
}
