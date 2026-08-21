using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Media;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Media;

[Collection(nameof(CollectionForIntegrationTests))]
public class MediaControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/medias";
    private readonly Faker _bogus;

    public MediaControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetAriaUri_Anonymous_Ok()
    {
        var uriAliases = await PrepareUriAliases();
        var firstUriAlias = uriAliases.Single(u => u.Type == UriType.BoulderGym);

        var response = await Client().GetAsync($"{_baseUrl}/{firstUriAlias.Alias}/{firstUriAlias.Type}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UriAliasDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        var boulderGym = await BiBaBoulderDbContext.BoulderGyms.FindAsync([firstUriAlias.BoulderGymId, TestContext.Current.CancellationToken], TestContext.Current.CancellationToken);
        Assert.NotNull(boulderGym);
    }

    private async Task<ICollection<UriAlias>> PrepareUriAliases()
    {
        var boulderGym = new BoulderGymBuilder()
            .SetName(_bogus.Company.CompanyName())
            .SetDescription(_bogus.Lorem.Sentence())
            .SetImportantInfo(_bogus.Lorem.Paragraph())
            .SetPreviewImageUri(_bogus.Image.PicsumUrl())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var uriAliasBoulderGym = new UriAliasBuilder(_bogus.Lorem.Slug())
            .SetType(UriType.BoulderGym)
            .SetBoulderGym(boulderGym)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAliasBoulderGym);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName(_bogus.Company.CompanyName())
            .SetDescription(_bogus.Lorem.Sentence())
            .SetImportantInfo(_bogus.Lorem.Paragraph())
            .SetPreviewImageUri(_bogus.Image.PicsumUrl())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var uriAliasOutdoorArea = new UriAliasBuilder(_bogus.Lorem.Slug())
            .SetType(UriType.OutdoorArea)
            .SetOutdoorArea(outdoorArea)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAliasOutdoorArea);

        return [uriAliasBoulderGym, uriAliasOutdoorArea];
    }
}
