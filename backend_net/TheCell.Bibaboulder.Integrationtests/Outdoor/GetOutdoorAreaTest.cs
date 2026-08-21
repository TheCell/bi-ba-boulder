using System.Net.Http.Json;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;

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
}
