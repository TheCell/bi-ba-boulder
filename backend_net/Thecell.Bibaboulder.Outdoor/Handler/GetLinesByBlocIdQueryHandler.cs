using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetLinesByBlocIdQueryHandler : IQueryHandler<GetLinesByBlocIdQuery, ICollection<LineDto>>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetLinesByBlocIdQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<LineDto>> HandleAsync(GetLinesByBlocIdQuery query)
    {
        var lines = await _dbContext.Lines
            .AsNoTracking()
            .Where(l => l.BlocId == query.BlocId)
            .ToListAsync();

        return lines.Select(l => l.MapToLineDto()).ToList();
    }
}
