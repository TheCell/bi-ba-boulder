using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class MediaControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/bouldergym";
    private readonly Faker _bogus;

    public MediaControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetBoulderGyms_Anonymous_Ok()
    {
        var boulderGyms = await PrepareBoulderGyms();

        var response = await Client().GetAsync(_baseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<BoulderGymDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        foreach (var boulderGym in boulderGyms)
        {
            var bg = result.Single(s => s.Id == boulderGym.Id);
            BoulderGymAssertion.Assert(boulderGym, bg);
        }
    }

    [Fact]
    public async Task GetBoulderGym_Anonymous_Ok()
    {
        var boulderGyms = await PrepareBoulderGyms();
        var boulderGym = boulderGyms.First();

        var response = await Client().GetAsync($"{_baseUrl}/{boulderGym.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        BoulderGymAssertion.Assert(boulderGym, result);
    }

    private async Task<List<BoulderGym>> PrepareBoulderGyms()
    {
        var boulderGyms = new List<BoulderGym>();
        for (var i = 0; i < 5; i++)
        {
            var spraywalls = new List<Thecell.Bibaboulder.Model.Model.Indoor.Spraywall>();
            for (var j = 0; j < 3; j++)
            {
                spraywalls.Add(new SpraywallBuilder()
                    .SetName(_bogus.Lorem.Slug())
                    .SetIsArchived(_bogus.Random.Bool())
                    .SetDescription(_bogus.Lorem.Sentence())
                    .SetPreviewImageUri(_bogus.Image.PicsumUrl())
                    .Build());
            }

            boulderGyms.Add(new BoulderGymBuilder()
                .SetName(_bogus.Company.CompanyName())
                .SetDescription(_bogus.Lorem.Sentence())
                .SetImportantInfo(_bogus.Lorem.Paragraph())
                .SetPreviewImageUri(_bogus.Image.PicsumUrl())
                .SetSpraywalls(spraywalls)
                .Build());
        }

        await BiBaBoulderDbContext.InsertEntitiesAndSaveChangesAsync(boulderGyms);

        return boulderGyms;
    }
}
