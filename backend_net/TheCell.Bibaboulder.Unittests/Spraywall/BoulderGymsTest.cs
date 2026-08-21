using System;
using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class BoulderGymsTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public BoulderGymsTest()
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
    public async Task GetBoulderGym_Ok()
    {
        var spraywall1 = new SpraywallBuilder()
            .SetName("Wall A")
            .SetIsArchived(true)
            .SetPreviewImageUri("https://example.com/wall-a.jpg")
            .Build();
        var spraywall2 = new SpraywallBuilder()
            .SetName("Wall B")
            .SetIsArchived(false)
            .SetPreviewImageUri("https://example.com/wall-b.jpg")
            .SetCreatedDate(DateTime.UtcNow.AddDays(-5))
            .Build();
        var spraywall3 = new SpraywallBuilder()
            .SetName("Wall B")
            .SetIsArchived(false)
            .SetCreatedDate(DateTime.UtcNow.AddDays(-3))
            .SetPreviewImageUri("https://example.com/wall-c.jpg")
            .Build();

        var boulderGym = new BoulderGymBuilder()
            .SetName("Test Gym")
            .SetDescription("Just a test")
            .SetImportantInfo("hear ye hear ye")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .SetSpraywalls([spraywall1, spraywall2, spraywall3])
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var handler = new GetBoulderGymQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBoulderGymQuery { Id = boulderGym.Id });

        BoulderGymAssertion.Assert(boulderGym, result);
        SpraywallAssertion.Assert(spraywall2, result.Spraywalls.First());
        SpraywallAssertion.Assert(spraywall1, result.Spraywalls.Last());
    }

    [Fact]
    public async Task GetBoulderGyms_Ok()
    {
        var spraywall1 = new SpraywallBuilder()
            .SetName("Wall A")
            .SetIsArchived(true)
            .SetPreviewImageUri("https://example.com/wall-a.jpg")
            .Build();
        var boulderGym1 = new BoulderGymBuilder()
            .SetName("Test Gym")
            .SetDescription("Just a test")
            .SetImportantInfo("hear ye hear ye")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .SetSpraywalls([spraywall1])
            .Build();

        var spraywall2 = new SpraywallBuilder()
            .SetName("Wall B")
            .SetIsArchived(false)
            .SetPreviewImageUri("https://example.com/wall-b.jpg")
            .Build();
        var boulderGym2 = new BoulderGymBuilder()
            .SetName("Test Gym 2")
            .SetDescription("Just a test")
            .SetImportantInfo("hear ye hear ye")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .SetSpraywalls([spraywall2])
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([boulderGym1, boulderGym2]);

        var handler = new GetBoulderGymQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBoulderGymQuery { Id = boulderGym2.Id });

        BoulderGymAssertion.Assert(boulderGym2, result);
        Assert.Single(result.Spraywalls);
        SpraywallAssertion.Assert(spraywall2, result.Spraywalls.First());
    }
}
