using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetSectorQueryHandler : IQueryHandler<GetSectorQuery, SectorDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetSectorQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<SectorDto> HandleAsync(GetSectorQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var sector = await _dbContext.Sectors
            .AsNoTracking()
            .Include(item => item.OutdoorAreas)
            .SingleOrDefaultAsync(s => s.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        sector.OutdoorAreas = sector.OutdoorAreas.Where(area => area.IsVisibleTo(currentUser)).ToList();

        return sector.MapToSectorDto();
    }
}
