using System.Collections.Generic;
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

public class GetOutdoorAreasQueryHandler : IQueryHandler<GetOutdoorAreasQuery, ICollection<OutdoorAreaDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetOutdoorAreasQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ICollection<OutdoorAreaDto>> HandleAsync(GetOutdoorAreasQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var outdoorAreas = await _dbContext.OutdoorAreas
            .AsNoTracking()
            .WhereVisibleTo(currentUser)
            .Include(outdoorArea => outdoorArea.Sectors)
            .ToListAsync();

        return outdoorAreas.Select(outdoorArea => outdoorArea.MapToOutdoorAreaDto()).ToList();
    }
}
