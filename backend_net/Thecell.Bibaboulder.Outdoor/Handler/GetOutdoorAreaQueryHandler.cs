using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetOutdoorAreaQueryHandler : IQueryHandler<GetOutdoorAreaQuery, OutdoorAreaDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetOutdoorAreaQueryHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<OutdoorAreaDto> HandleAsync(GetOutdoorAreaQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var outdoorArea = await _dbContext.OutdoorAreas
            .AsNoTracking()
            .WhereVisibleTo(currentUser)
            .Include(area => area.Sectors)
            .SingleOrDefaultAsync(area => area.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return outdoorArea.MapToOutdoorAreaDto();
    }
}
