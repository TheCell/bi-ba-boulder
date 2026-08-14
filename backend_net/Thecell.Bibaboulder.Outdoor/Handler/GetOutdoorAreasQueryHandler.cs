using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetOutdoorAreasQueryHandler : IQueryHandler<GetOutdoorAreasQuery, ICollection<OutdoorAreaDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetOutdoorAreasQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<OutdoorAreaDto>> HandleAsync(GetOutdoorAreasQuery query)
    {
        var outdoorAreas = await _dbContext.OutdoorAreas
            .AsNoTracking()
            .Include(outdoorArea => outdoorArea.Sectors)
            .ToListAsync();

        return outdoorAreas.Select(outdoorArea => outdoorArea.MapToOutdoorAreaDto()).ToList();
    }
}
