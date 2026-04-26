using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class GetSpraywallsQueryHandler : IQueryHandler<GetSpraywallsQuery, ICollection<SpraywallDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSpraywallsQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<SpraywallDto>> HandleAsync(GetSpraywallsQuery query)
    {
        var spraywalls = await _dbContext.Spraywalls
            .AsNoTracking()
            .ToListAsync();

        return spraywalls.Select(s => s.MapToSpraywallDto()).ToList();
    }
}
