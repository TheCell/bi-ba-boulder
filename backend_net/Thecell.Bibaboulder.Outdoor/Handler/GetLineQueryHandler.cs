using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetLineQueryHandler : IQueryHandler<GetLineQuery, LineDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetLineQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LineDto> HandleAsync(GetLineQuery query)
    {
        var line = await _dbContext.Lines
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return line.MapToLineDto();
    }
}