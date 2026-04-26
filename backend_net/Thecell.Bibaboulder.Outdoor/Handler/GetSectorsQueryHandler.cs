using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetSectorsQueryHandler : IQueryHandler<GetSectorsQuery, ICollection<SectorDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSectorsQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<SectorDto>> HandleAsync(GetSectorsQuery query)
    {
        var sectors = await _dbContext.Sectors
            .AsNoTracking()
            .ToListAsync();

        var listOfSectors = new List<SectorDto>();
        foreach (var sector in sectors)
        {
            listOfSectors.Add(sector.MapToSectorDto());
        }
        return listOfSectors;
    }
}
