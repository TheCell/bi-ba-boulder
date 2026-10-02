using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Media.Handler;

public class GetUriAliasesQueryHandler : IQueryHandler<GetUriAliasesQuery, ICollection<UriAliasAdministrationDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUriAliasesQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ICollection<UriAliasAdministrationDto>> HandleAsync(GetUriAliasesQuery query)
    {
        var currentUser = await UriAliasHandlerUtilities.EnsureContentAdministratorAsync(_currentUserService);

        var uriAliases = await _dbContext.UriAliases
            .AsNoTracking()
            .Include(ua => ua.BoulderGym)
            .Include(ua => ua.OutdoorArea)
            .OrderBy(uriAlias => uriAlias.Type)
            .ThenBy(uriAlias => uriAlias.Alias)
            .ToListAsync();

        return uriAliases
            .Where(uriAlias => uriAlias.OutdoorArea is null || uriAlias.OutdoorArea.IsVisibleTo(currentUser))
            .Select(uriAlias => uriAlias.MapToUriAliasDto())
            .ToList();
    }
}
