using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Dto;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class GetOutdoorAreasTest : OutdoorAreaIntegrationTestBase
{
    public GetOutdoorAreasTest(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetOutdoorAreas_WithMultipleSectorsAsAnonymous_Ok()
    {
        var outdoorArea = await PrepareOutdoorArea();

        var response = await Client().GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<OutdoorAreaDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var outdoorAreaDto = result.Single(area => area.Id == outdoorArea.Id);
        OutdoorAreaAssertion.Assert(outdoorArea, outdoorAreaDto);
        Assert.Equal(2, outdoorAreaDto.Sectors.Count);
    }
}
