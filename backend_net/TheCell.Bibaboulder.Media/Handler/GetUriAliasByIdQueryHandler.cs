using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Media.Handler;

public class GetUriAliasByIdQueryHandler : IQueryHandler<GetUriAliasByIdQuery, UriAliasAdministrationDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUriAliasByIdQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<UriAliasAdministrationDto> HandleAsync(GetUriAliasByIdQuery query)
    {
        await UriAliasHandlerUtilities.EnsureContentAdministratorAsync(_currentUserService);

        var uriAlias = await _dbContext.UriAliases
            .AsNoTracking()
            .Include(ua => ua.BoulderGym)
            .Include(ua => ua.OutdoorArea)
            .Where(uriAlias => uriAlias.Id == query.Id)
            .Select(uriAlias => uriAlias.MapToUriAliasDto())
            .SingleOrDefaultAsync();
        NotFoundException.ThrowIfNull(uriAlias, nameof(UriAliasAdministrationDto), query.Id);

        return uriAlias;
    }
}
