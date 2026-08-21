using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class GetBoulderGymsQueryHandler : IQueryHandler<GetBoulderGymsQuery, ICollection<BoulderGymDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderGymsQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<BoulderGymDto>> HandleAsync(GetBoulderGymsQuery query)
    {
        var boulderGyms = await _dbContext.BoulderGyms
            .AsNoTracking()
            .Include(gym => gym.Spraywalls.OrderBy(s => s.IsArchived).ThenBy(s => s.CreatedDate))
            .ToListAsync();

        return boulderGyms.Select(gym => gym.MapToBoulderGymDto()).ToList();
    }
}
