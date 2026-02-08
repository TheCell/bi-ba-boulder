using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Spraywall.Testing;

public class GetTestingQueryHandler : IQueryHandler<GetTestingQuery, TestDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetTestingQueryHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TestDto> HandleAsync(GetTestingQuery request)
    {
        var count = await _dbContext.SpraywallProblems.CountAsync();
        return new TestDto
        {
            Id = Guid.NewGuid(),
            SpraywallCount = count
        };
    }
}
