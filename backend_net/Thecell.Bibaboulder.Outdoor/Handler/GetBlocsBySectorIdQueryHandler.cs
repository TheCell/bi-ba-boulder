using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetBlocsBySectorIdQueryHandler : IQueryHandler<GetBlocsBySectorIdQuery, ICollection<BlocDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBlocsBySectorIdQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<BlocDto>> HandleAsync(GetBlocsBySectorIdQuery query)
    {
        var blocs = await _dbContext.Blocs
            .AsNoTracking()
            .Where(b => b.SectorId == query.SectorId)
            .ToListAsync();

        return blocs.Select(b => b.MapToBlocDto()).ToList();
    }
}
