using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetSectorQueryHandler : IQueryHandler<GetSectorQuery, SectorDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSectorQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SectorDto> HandleAsync(GetSectorQuery query)
    {
        var sector = await _dbContext.Sectors
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return sector.MapToSectorDto();
    }
}
