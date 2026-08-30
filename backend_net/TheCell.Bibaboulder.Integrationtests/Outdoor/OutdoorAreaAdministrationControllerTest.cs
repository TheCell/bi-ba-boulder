using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class OutdoorAreaAdministrationControllerTest : BaseTest
{
    private const string BaseUrl = "/api/OutdoorAreas";
    private readonly Faker _bogus;

    public OutdoorAreaAdministrationControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateOutdoorArea_Anonymous_Unauthorized()
    {
        var command = new CreateOutdoorAreaCommand { Name = _bogus.Lorem.Slug() };

        var response = await Client().PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOutdoorArea_User_Forbidden()
    {
        var command = new CreateOutdoorAreaCommand { Name = _bogus.Lorem.Slug() };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateOutdoorArea_WithSectors_Ok()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.ContentAdmin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var sector = new SectorBuilder()
            .SetName("The Shield")
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new CreateOutdoorAreaCommand
        {
            Name = _bogus.Lorem.Slug(),
            SectorIds = [sector.Id]
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: user.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OutdoorAreaDto>(cancellationToken: TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.NotNull(result);
        Assert.Single(result.Sectors);
        Assert.Equal(sector.Id, result.Sectors.Single().Id);
    }

    [Fact]
    public async Task CreateOutdoorArea_UnknownSector_BadRequest()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateOutdoorAreaCommand
        {
            Name = _bogus.Lorem.Slug(),
            SectorIds = [System.Guid.NewGuid()]
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOutdoorArea_Ok()
    {
        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Original")
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new UpdateOutdoorAreaCommand { Name = _bogus.Lorem.Slug(), Version = outdoorArea.Version };

        var client = AuthenticatedClient(role: AuthorizationRoles.ContentAdmin);
        var response = await client.PutAsync($"{BaseUrl}/{outdoorArea.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OutdoorAreaDto>(cancellationToken: TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(outdoorArea.Version + 1, result.Version);
    }

    [Fact]
    public async Task UpdateOutdoorArea_StaleVersion_Conflict()
    {
        var outdoorArea = new OutdoorAreaBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new UpdateOutdoorAreaCommand { Name = _bogus.Lorem.Slug(), Version = outdoorArea.Version + 99 };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.PutAsync($"{BaseUrl}/{outdoorArea.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOutdoorArea_Ok()
    {
        var outdoorArea = new OutdoorAreaBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{outdoorArea.Id}")
        {
            Content = GetJsonHttpBody(new DeleteOutdoorAreaCommand { Version = outdoorArea.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var exists = await BiBaBoulderDbContext.OutdoorAreas.AnyAsync(area => area.Id == outdoorArea.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteOutdoorArea_User_Forbidden()
    {
        var outdoorArea = new OutdoorAreaBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{outdoorArea.Id}")
        {
            Content = GetJsonHttpBody(new DeleteOutdoorAreaCommand { Version = outdoorArea.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
