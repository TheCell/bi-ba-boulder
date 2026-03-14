using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetSectorsTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly Faker _bogus;

    public GetSectorsTest()
    {
        _dbContext = new DbContextMock().Build();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetSectors_Ok()
    {
        var sectors = await PrepareData();

        var query = new GetSectorsQuery();

        var handler = new GetSectorsQueryHandler(_dbContext);
        var result = await handler.HandleAsync(query);

        Assert.Equal(sectors.Count, result.Count);
        foreach (var sector in sectors)
        {
            var sectorDto = result.SingleOrDefault(s => s.Id == sector.Id);
            Assert.NotNull(sectorDto);
            SectorAssertion.Assert(sector, sectorDto);
        }
    }

    private async Task<List<Sector>> PrepareData()
    {
        var sectors = new List<Sector>();
        for (var i = 0; i < 10; i++)
        {
            var sector = new SectorBuilder()
                .SetName(_bogus.Company.CompanyName())
                .SetDescription(_bogus.Lorem.Sentence())
                .Build();
            sectors.Add(sector);
            await _dbContext.InsertEntityAsync(sector);
        }
        return sectors;
    }
}
