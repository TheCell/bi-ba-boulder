using System.Net;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class AdministrationControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Administration";

    public AdministrationControllerTest(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Index_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Index_User_Forbidden()
    {
        var client = AuthenticatedClient(role: AuthorizationRoles.User);

        var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Index_Admin_Ok()
    {
        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);

        var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
