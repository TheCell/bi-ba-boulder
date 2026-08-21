using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Model.Media;
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
            .SingleOrDefaultAsync(alias => alias.Alias == query.Name && alias.Type == query.Type)
            .ThrowIfNullAsync(null);

        NotFoundException.ThrowIfNull(uriAlias, nameof(UriAlias), uriAlias?.Id);

        return new UriAliasDto { Id = uriAlias.Id };
    }
}
