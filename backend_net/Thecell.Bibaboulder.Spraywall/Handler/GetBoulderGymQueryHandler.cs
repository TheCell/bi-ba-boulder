using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class GetBoulderGymQueryHandler : IQueryHandler<GetBoulderGymQuery, BoulderGymDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderGymQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BoulderGymDto> HandleAsync(GetBoulderGymQuery query)
    {
        var boulderGym = await _dbContext.BoulderGyms
            .AsNoTracking()
            .Include(gym => gym.Spraywalls.OrderBy(s => s.IsArchived).ThenBy(s => s.CreatedDate))
            .SingleOrDefaultAsync(gym => gym.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return boulderGym.MapToBoulderGymDto();
    }
}
