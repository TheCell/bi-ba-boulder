using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetOutdoorAreasTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetOutdoorAreasTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetOutdoorAreas_EmptyResult_Ok()
    {
        var handler = new GetOutdoorAreasQueryHandler(_dbContext);

        var result = await handler.HandleAsync(new GetOutdoorAreasQuery());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetOutdoorAreas_WithMultipleSectors_Ok()
    {
        var firstSector = new SectorBuilder()
            .SetName("First Sector")
            .SetImportantInfo("Bring a crash pad")
            .SetIsPublic(true)
            .SetCoordinates("46.9914628, 7.5589870")
            .SetPreviewImageUri("https://example.com/first-preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/first.jpg").SetResourceType(ResourceType.Image).Build()])
            .Build();
        var secondSector = new SectorBuilder()
            .SetName("Second Sector")
            .SetImportantInfo("Dry conditions only")
            .SetIsPublic(false)
            .SetCoordinates("46.9929293, 7.5582829")
            .SetPreviewImageUri("https://example.com/second-preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/second.mp4").SetResourceType(ResourceType.Video).Build()])
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([firstSector, secondSector]);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Lindental")
            .SetDescription("Sandstone bouldering")
            .SetImportantInfo("Respect access rules")
            .SetPreviewImageUri("https://example.com/area-preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/area.jpg").SetResourceType(ResourceType.Image).Build()])
            .SetSectors([firstSector, secondSector])
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var handler = new GetOutdoorAreasQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetOutdoorAreasQuery());

        var outdoorAreaDto = Assert.Single(result);
        OutdoorAreaAssertion.Assert(outdoorArea, outdoorAreaDto);
        Assert.Equal(2, outdoorAreaDto.Sectors.Count);
        Assert.Contains(outdoorAreaDto.Sectors, sector => sector.Id == firstSector.Id);
        Assert.Contains(outdoorAreaDto.Sectors, sector => sector.Id == secondSector.Id);
    }
}
