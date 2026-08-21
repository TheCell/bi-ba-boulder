using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Extensions;

namespace TheCell.Bibaboulder.Media.Handler;

public class GetUriAliasQueryHandler : IQueryHandler<GetUriAliasQuery, UriAliasDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetUriAliasQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UriAliasDto> HandleAsync(GetUriAliasQuery query)
    {
        var uriAlias = await _dbContext.UriAliases
            .AsNoTracking()
            .SingleOrDefaultAsync(alias => alias.Alias == query.Alias && alias.Type == query.Type)
            .ThrowIfNullAsync();

        return new UriAliasDto { Id = uriAlias.Id };
    }
}
