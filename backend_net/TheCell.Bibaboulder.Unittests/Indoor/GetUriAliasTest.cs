using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Media.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Indoor;

public class GetUriAliasTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetUriAliasTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetUriAlias_NotFound_Ok()
    {
        var handler = new GetUriAliasQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetUriAliasQuery { Alias = "test-alias", Type = UriType.OutdoorArea });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUriAlias_Ok()
    {
        var boulderGym = new BoulderGymBuilder()
            .SetName("Test Gym")
            .SetDescription("Just a test")
            .SetImportantInfo("hear ye hear ye")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Test outdoor area")
            .SetDescription("Just a test")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var uriAlias1 = new UriAliasBuilder("test-boulder-gym")
            .SetBoulderGym(boulderGym)
            .SetType(UriType.BoulderGym)
            .Build();

        var uriAlias2 = new UriAliasBuilder("test-outdoor-area")
            .SetOutdoorArea(outdoorArea)
            .SetType(UriType.OutdoorArea)
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([uriAlias1, uriAlias2]);

        var handler = new GetUriAliasQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetUriAliasQuery { Alias = "test-boulder-gym", Type = UriType.BoulderGym });

        Assert.NotNull(result);
        Assert.Equal(boulderGym.Id, result.Id);
    }

    [Fact]
    public async Task GetUriAlias_WrongType_Ok()
    {
        var boulderGym = new BoulderGymBuilder()
            .SetName("Test Gym")
            .SetDescription("Just a test")
            .SetImportantInfo("hear ye hear ye")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Test outdoor area")
            .SetDescription("Just a test")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var uriAlias1 = new UriAliasBuilder("test-boulder-gym")
            .SetBoulderGym(boulderGym)
            .SetType(UriType.BoulderGym)
            .Build();

        var uriAlias2 = new UriAliasBuilder("test-outdoor-area")
            .SetOutdoorArea(outdoorArea)
            .SetType(UriType.OutdoorArea)
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([uriAlias1, uriAlias2]);

        var handler = new GetUriAliasQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetUriAliasQuery { Alias = "test-boulder-gym", Type = UriType.OutdoorArea });

        Assert.Null(result);
    }
}
