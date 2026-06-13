using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogQueryHandler : IQueryHandler<GetBoulderLogQuery, BoulderLogDto?>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BoulderLogDto?> HandleAsync(GetBoulderLogQuery query)
    {
        var boulderLog = await _dbContext.BoulderLogs
            .FindAsync(query.Id);

        if (boulderLog == null)
        {
            return null;
        }

        return boulderLog.MapToBoulderLogDto();
    }
}
