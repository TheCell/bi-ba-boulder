using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetOutdoorAreaQueryHandler : IQueryHandler<GetOutdoorAreaQuery, OutdoorAreaDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetOutdoorAreaQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OutdoorAreaDto> HandleAsync(GetOutdoorAreaQuery query)
    {
        var outdoorArea = await _dbContext.OutdoorAreas
            .AsNoTracking()
            .Include(area => area.Sectors)
            .SingleOrDefaultAsync(area => area.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return outdoorArea.MapToOutdoorAreaDto();
    }
}
