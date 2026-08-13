using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

public abstract class OutdoorAreaIntegrationTestBase : BaseTest
{
    protected const string BaseUrl = "/api/OutdoorAreas";

    protected OutdoorAreaIntegrationTestBase(IntegrationTestFactory factory) : base(factory)
    {
    }

    protected async Task<OutdoorArea> PrepareOutdoorArea()
    {
        var firstSector = new SectorBuilder()
            .SetName("First Sector")
            .SetDescription("First sector description")
            .SetImportantInfo("Bring a crash pad")
            .SetIsPublic(true)
            .SetCoordinates("46.9914628, 7.5589870")
            .SetPreviewImageUri("https://example.com/first-preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/first.jpg").SetResourceType(ResourceType.Image).Build()])
            .Build();
        var secondSector = new SectorBuilder()
            .SetName("Second Sector")
            .SetDescription("Second sector description")
            .SetImportantInfo("Dry conditions only")
            .SetIsPublic(false)
            .SetCoordinates("46.9929293, 7.5582829")
            .SetPreviewImageUri("https://example.com/second-preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/second.mp4").SetResourceType(ResourceType.Video).Build()])
            .Build();
        await BiBaBoulderDbContext.InsertEntitiesAndSaveChangesAsync([firstSector, secondSector]);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Lindental")
            .SetDescription("Sandstone bouldering")
            .SetImportantInfo("Respect access rules")
            .SetPreviewImageUri("https://example.com/area-preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/area.jpg").SetResourceType(ResourceType.Image).Build()])
            .SetSectors([firstSector, secondSector])
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        return outdoorArea;
    }
}