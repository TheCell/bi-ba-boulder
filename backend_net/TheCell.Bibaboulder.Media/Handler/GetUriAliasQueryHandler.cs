using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Enums;

namespace TheCell.Bibaboulder.Media.Handler;

public class GetUriAliasQueryHandler : IQueryHandler<GetUriAliasQuery, UriAliasDto?>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetUriAliasQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UriAliasDto?> HandleAsync(GetUriAliasQuery query)
    {
        var uriAlias = await _dbContext.UriAliases
            .AsNoTracking()
            .SingleOrDefaultAsync(alias => alias.Alias == query.Alias && alias.Type == query.Type);

        if (uriAlias is null)
        {
            return null;
        }

        switch (uriAlias.Type)
        {
            case UriType.BoulderGym:
                if (uriAlias.BoulderGymId is null)
                {
                    throw new InvalidOperationException($"UriAlias with Id {uriAlias.Id} has type {UriType.BoulderGym} but no BoulderGymId.");
                }
                return new UriAliasDto { Id = uriAlias.BoulderGymId.Value };
            case UriType.OutdoorArea:
                if (uriAlias.OutdoorAreaId is null)
                {
                    throw new InvalidOperationException($"UriAlias with Id {uriAlias.Id} has type {UriType.OutdoorArea} but no OutdoorAreaId.");
                }
                return new UriAliasDto { Id = uriAlias.OutdoorAreaId.Value };
            default:
                throw new InvalidOperationException($"UriAlias with Id {uriAlias.Id} has an unknown type {uriAlias.Type}.");
        }
    }
}
