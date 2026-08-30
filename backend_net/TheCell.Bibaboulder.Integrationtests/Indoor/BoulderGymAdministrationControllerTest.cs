using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Indoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class BoulderGymAdministrationControllerTest : BaseTest
{
    private const string BaseUrl = "/api/BoulderGym";
    private readonly Faker _bogus;

    public BoulderGymAdministrationControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateBoulderGym_Anonymous_Unauthorized()
    {
        var command = new CreateBoulderGymCommand { Name = _bogus.Lorem.Slug() };

        var response = await Client().PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBoulderGym_User_Forbidden()
    {
        var command = new CreateBoulderGymCommand { Name = _bogus.Lorem.Slug() };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(AuthorizationRoles.ContentAdmin)]
    [InlineData(AuthorizationRoles.Admin)]
    public async Task CreateBoulderGym_Authorized_Ok(string role)
    {
        var user = new UserBuilder()
            .SetRoles(role)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateBoulderGymCommand
        {
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            PreviewImageUri = _bogus.Internet.Url(),
            ImageUris = [_bogus.Internet.Url()]
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: role, username: user.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Single(result.Images);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public async Task CreateBoulderGym_InvalidImageUri_BadRequest()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateBoulderGymCommand { Name = _bogus.Lorem.Slug(), ImageUris = ["not-a-url"] };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoulderGym_Ok()
    {
        var boulderGym = new BoulderGymBuilder()
            .SetName("Original")
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand
        {
            Name = _bogus.Lorem.Slug(),
            Version = boulderGym.Version
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.ContentAdmin);
        var response = await client.PutAsync($"{BaseUrl}/{boulderGym.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(boulderGym.Version + 1, result.Version);
    }

    [Fact]
    public async Task UpdateBoulderGym_StaleVersion_Conflict()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand { Name = _bogus.Lorem.Slug(), Version = boulderGym.Version + 99 };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.PutAsync($"{BaseUrl}/{boulderGym.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBoulderGym_Ok()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{boulderGym.Id}")
        {
            Content = GetJsonHttpBody(new DeleteBoulderGymCommand { Version = boulderGym.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var exists = await BiBaBoulderDbContext.BoulderGyms.AnyAsync(gym => gym.Id == boulderGym.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteBoulderGym_User_Forbidden()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{boulderGym.Id}")
        {
            Content = GetJsonHttpBody(new DeleteBoulderGymCommand { Version = boulderGym.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var exists = await BiBaBoulderDbContext.BoulderGyms.AnyAsync(gym => gym.Id == boulderGym.Id, TestContext.Current.CancellationToken);
        Assert.True(exists);
    }
}
