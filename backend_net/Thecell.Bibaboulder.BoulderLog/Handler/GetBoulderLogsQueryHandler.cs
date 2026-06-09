using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogsQueryHandler : IQueryHandler<GetBoulderLogsQuery, ICollection<BoulderLogDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogsQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<BoulderLogDto>> HandleAsync(GetBoulderLogsQuery query)
    {
        var logs = await _dbContext.BoulderLogs
            .AsNoTracking()
            .Where(b => b.UserId == query.UserId)
            .Select(b => new BoulderLogDto
            {
                Id = b.Id,
                IsSent = b.IsSent,
                IsProject = b.IsProject,
                Rating = b.Rating,
                FontGrade = b.FontGrade,
                UserId = b.UserId,
                SpraywallProblemId = b.SpraywallProblemId,
                Version = b.Version,
            })
            .ToListAsync();

        return logs;
    }
}
