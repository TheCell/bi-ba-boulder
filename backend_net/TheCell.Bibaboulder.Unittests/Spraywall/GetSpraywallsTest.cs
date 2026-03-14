using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class GetSpraywallsTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSpraywallsTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetSpraywalls_EmptyResult_Ok()
    {
        var handler = new GetSpraywallsQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetSpraywallsQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSpraywalls_Ok()
    {
        var spraywall1 = new SpraywallBuilder().SetName("Wall A").Build();
        var spraywall2 = new SpraywallBuilder().SetName("Wall B").Build();
        await _dbContext.InsertEntitiesAsync([spraywall1, spraywall2]);

        var handler = new GetSpraywallsQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetSpraywallsQuery());

        Assert.Equal(2, result.Count);
        SpraywallAssertion.Assert(spraywall1, result.Single(s => s.Id == spraywall1.Id));
        SpraywallAssertion.Assert(spraywall2, result.Single(s => s.Id == spraywall2.Id));
    }
}
