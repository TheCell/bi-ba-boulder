using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetBlocQueryHandler : IQueryHandler<GetBlocQuery, BlocDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBlocQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BlocDto> HandleAsync(GetBlocQuery query)
    {
        var bloc = await _dbContext.Blocs
            .Include(b => b.AdditionalParts)
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        return bloc.MapToBlocDto();
    }
}
