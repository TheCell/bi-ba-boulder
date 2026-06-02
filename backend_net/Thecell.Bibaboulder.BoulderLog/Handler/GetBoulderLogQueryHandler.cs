using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogQueryHandler : IQueryHandler<GetBoulderLogQuery, BoulderLogDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BoulderLogDto> HandleAsync(GetBoulderLogQuery query)
    {
        var boulderLog = await _dbContext.BoulderLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == query.Id);

        NotFoundException.ThrowIfNull(boulderLog, nameof(Model.Model.BoulderLog), query.Id);

        return boulderLog.MapToBoulderLogDto();
    }
}
