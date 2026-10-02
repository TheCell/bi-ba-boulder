using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class GetOutdoorAreaTest : OutdoorAreaIntegrationTestBase
{
    public GetOutdoorAreaTest(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetOutdoorArea_WithMultipleSectorsAsAnonymous_Ok()
    {
        var outdoorArea = await PrepareOutdoorArea();

        var response = await Client().GetAsync($"{BaseUrl}/{outdoorArea.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OutdoorAreaDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        OutdoorAreaAssertion.Assert(outdoorArea, result);
        Assert.Equal(2, result.Sectors.Count);
    }

    [Fact]
    public async Task GetOutdoorArea_PrivateAreaAsAnonymous_NotFound()
    {
        var privateArea = new OutdoorAreaBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(privateArea);

        var response = await Client().GetAsync($"{BaseUrl}/{privateArea.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
