using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class GetSpraywallQueryHandler : IQueryHandler<GetSpraywallQuery, SpraywallDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSpraywallQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SpraywallDto> HandleAsync(GetSpraywallQuery query)
    {
        var spraywall = await _dbContext.Spraywalls
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return spraywall.MapToSpraywallDto();
    }
}
