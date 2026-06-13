using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogBySpraywallQueryHandler : IQueryHandler<GetBoulderLogBySpraywallQuery, BoulderLogDto?>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogBySpraywallQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BoulderLogDto?> HandleAsync(GetBoulderLogBySpraywallQuery query)
    {
        var boulderLog = await _dbContext.BoulderLogs
            .SingleOrDefaultAsync(bl => bl.SpraywallProblemId == query.SpraywallProblemId);

        return boulderLog?.MapToBoulderLogDto();
    }
}
