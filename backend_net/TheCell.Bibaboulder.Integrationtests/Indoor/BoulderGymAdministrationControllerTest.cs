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
            PreviewImageUri = _bogus.Internet.Url()
                .Replace("https://", "")
                .Replace("http://", ""),
            ImageUris = [_bogus.Internet.Url()
                .Replace("https://", "")
                .Replace("http://", "")]
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: role, username: user.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);

        var boulderGym = await BiBaBoulderDbContext.BoulderGyms
            .Include(boulderGym => boulderGym.Media)
            .AsNoTracking()
            .SingleAsync(gym => gym.Id == result.Id, TestContext.Current.CancellationToken);

        Assert.Equal(command.Name, boulderGym.Name);
        Assert.Equal(command.Description, boulderGym.Description);
        Assert.Equal(command.ImportantInfo, boulderGym.ImportantInfo);
        Assert.Equal(command.PreviewImageUri, boulderGym.PreviewImageUri);
        Assert.Equal(command.ImageUris.Count, boulderGym.Media.Count);
        foreach (var uri in command.ImageUris)
        {
            Assert.Contains(boulderGym.Media, media => media.Uri == uri);
        }
        Assert.Equal(1, boulderGym.Version);
        Assert.Equal(user.Id, boulderGym.CreatedUserId);
    }

    [Fact]
    public async Task CreateBoulderGym_InvalidImageUri_BadRequest()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateBoulderGymCommand { Name = _bogus.Lorem.Slug(), ImageUris = ["https://other-url/not-a-url"] };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoulderGym_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var boulderGym = new BoulderGymBuilder()
            .SetName("Original")
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand
        {
            Name = _bogus.Lorem.Slug(),
            Version = boulderGym.Version
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PutAsync($"{BaseUrl}/{boulderGym.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(boulderGym.Version + 1, result.Version);
    }

    [Fact]
    public async Task UpdateBoulderGym_StaleVersion_Conflict()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand { Name = _bogus.Lorem.Slug(), Version = boulderGym.Version + 99 };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PutAsync($"{BaseUrl}/{boulderGym.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBoulderGym_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var spraywall = new SpraywallBuilder()
            .SetBoulderGym(boulderGym)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{boulderGym.Id}")
        {
            Content = GetJsonHttpBody(new DeleteBoulderGymCommand { Version = boulderGym.Version })
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var exists = await BiBaBoulderDbContext.BoulderGyms.AnyAsync(gym => gym.Id == boulderGym.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);

        var spraywallFromDb = await BiBaBoulderDbContext.Spraywalls
            .AsNoTracking()
            .SingleAsync(sw => sw.Id == spraywall.Id, TestContext.Current.CancellationToken);
        Assert.Null(spraywallFromDb.BoulderGymId);
    }

    [Fact]
    public async Task DeleteBoulderGym_UriAlias_InternalServerError()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.ContentAdmin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var uriAlias = new UriAliasBuilder("test-url")
            .SetBoulderGym(boulderGym)
            .SetType(Thecell.Bibaboulder.Model.Enums.UriType.BoulderGym)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{boulderGym.Id}")
        {
            Content = GetJsonHttpBody(new DeleteBoulderGymCommand { Version = boulderGym.Version })
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: user.Username);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
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

    [Fact]
    public async Task AddSpraywallToBoulderGym_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var spraywall = new SpraywallBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var command = new AddSpraywallToBoulderGymCommand
        {
            BoulderGymId = boulderGym.Id,
            SpraywallId = spraywall.Id
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PostAsync($"{BaseUrl}/{boulderGym.Id}/spraywalls", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Contains(result.Spraywalls, s => s.Id == spraywall.Id);

        var spraywallFromDb = await BiBaBoulderDbContext.Spraywalls
            .AsNoTracking()
            .SingleAsync(s => s.Id == spraywall.Id, TestContext.Current.CancellationToken);
        Assert.Equal(boulderGym.Id, spraywallFromDb.BoulderGymId);
    }

    [Fact]
    public async Task RemoveSpraywallFromBoulderGym_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var spraywall = new SpraywallBuilder().SetBoulderGymId(boulderGym.Id).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.DeleteAsync($"{BaseUrl}/{boulderGym.Id}/spraywalls/{spraywall.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderGymDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.DoesNotContain(result.Spraywalls, s => s.Id == spraywall.Id);

        var spraywallFromDb = await BiBaBoulderDbContext.Spraywalls
            .AsNoTracking()
            .SingleAsync(s => s.Id == spraywall.Id, TestContext.Current.CancellationToken);
        Assert.Null(spraywallFromDb.BoulderGymId);
    }
}
