using System.Net;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Indoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class AdministrationControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Administration";
    private readonly Faker _bogus;

    public AdministrationControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task Index_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_User_Forbidden()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var client = AuthenticatedClient(subjectId: user.OidcSubject);

        var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Index_Admin_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var client = AuthenticatedClient(subjectId: user.OidcSubject);

        var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
