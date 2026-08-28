using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Media.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Media;

[Collection(nameof(CollectionForIntegrationTests))]
public class MediasControllerTest : BaseTest
{
    private const string BaseUrl = "/api/Medias/uri-aliases";

    public MediasControllerTest(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUriAliases_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUriAliases_User_Forbidden()
    {
        var client = await CreateClientAsync(AuthorizationRoles.User);

        var response = await client.GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(AuthorizationRoles.ContentAdmin)]
    [InlineData(AuthorizationRoles.Admin)]
    public async Task GetUriAliases_ContentAdministratorEmptyList_Ok(string role)
    {
        var client = await CreateClientAsync(role);

        var response = await client.GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ICollection<UriAliasAdministrationDto>>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(AuthorizationRoles.ContentAdmin)]
    [InlineData(AuthorizationRoles.Admin)]
    public async Task GetUriAliases_ContentAdministrator_Ok(string role)
    {
        var boulderGym = new BoulderGymBuilder().SetName("Bimano").Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var uriAlias = new UriAliasBuilder("main-gym").SetType(UriType.BoulderGym).SetBoulderGym(boulderGym).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var client = await CreateClientAsync(role);

        var response = await client.GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ICollection<UriAliasAdministrationDto>>(
            cancellationToken: TestContext.Current.CancellationToken);

        var uriAliasDto = result?.FirstOrDefault(item => item.Id == uriAlias.Id);
        Assert.NotNull(uriAliasDto);
        UriAliasAssertion.Assert(uriAlias, uriAliasDto);
    }

    [Fact]
    public async Task CreateUriAlias_ContentAdministrator_NormalizesAndReturnsAlias()
    {
        var boulderGym = new BoulderGymBuilder().SetName("Bimano").Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);
        var command = new CreateUriAliasCommand
        {
            Alias = "  Main-Gym  ",
            TypeId = (long)UriType.BoulderGym,
            TargetId = boulderGym.Id
        };

        var response = await (await CreateClientAsync(AuthorizationRoles.ContentAdmin))
            .PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UriAliasAdministrationDto>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        var persistedAlias = await BiBaBoulderDbContext.UriAliases.SingleAsync(
            item => item.Id == result.Id,
            TestContext.Current.CancellationToken);

        UriAliasAssertion.Assert(persistedAlias, result);
        Assert.Equal("main-gym", result.Alias);
        Assert.Equal((long)UriType.BoulderGym, result.TypeId);
        Assert.Equal(boulderGym.Id, result.TargetId);
        Assert.Equal(boulderGym.Name, result.TargetName);
        Assert.Equal(1, result.Version);
    }

    [Fact]
    public async Task CreateUriAlias_DuplicateAlias_BadRequest()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        var uriAlias = new UriAliasBuilder("main-gym").SetType(UriType.BoulderGym).SetBoulderGym(boulderGym).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAlias);
        var command = new CreateUriAliasCommand
        {
            Alias = "MAIN-GYM",
            TypeId = (long)UriType.BoulderGym,
            TargetId = boulderGym.Id
        };

        var response = await (await CreateClientAsync(AuthorizationRoles.ContentAdmin))
            .PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUriAlias_TooLong_BadRequest()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderGym);
        var command = new CreateUriAliasCommand
        {
            Alias = new string('a', 101),
            TypeId = (long)UriType.BoulderGym,
            TargetId = boulderGym.Id
        };

        var response = await (await CreateClientAsync(AuthorizationRoles.ContentAdmin))
            .PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUriAlias_ChangesTypeAndTargetAsContentAdministrator_Ok()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        var outdoorArea = new OutdoorAreaBuilder().SetName("Magic Wood").Build();
        var uriAlias = new UriAliasBuilder("old-alias").SetType(UriType.BoulderGym).SetBoulderGym(boulderGym).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAlias);
        var command = new UpdateUriAliasCommand
        {
            Alias = "magic-wood",
            TypeId = (long)UriType.OutdoorArea,
            TargetId = outdoorArea.Id,
            Version = uriAlias.Version
        };

        var client = await CreateClientAsync(AuthorizationRoles.Admin);
        var response = await client
            .PutAsync($"{BaseUrl}/{uriAlias.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UriAliasAdministrationDto>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(outdoorArea.Id, result.TargetId);
        Assert.Equal((long)UriType.OutdoorArea, result.TypeId);
        Assert.Equal(2, result.Version);

        BiBaBoulderDbContext.UriAliases.Entry(uriAlias).State = EntityState.Detached;
        var persistedAlias = await BiBaBoulderDbContext.UriAliases.SingleAsync(
            item => item.Id == uriAlias.Id,
            TestContext.Current.CancellationToken);
        Assert.Null(persistedAlias.BoulderGymId);
        Assert.Equal(outdoorArea.Id, persistedAlias.OutdoorAreaId);
        UriAliasAssertion.Assert(persistedAlias, result);
    }

    [Fact]
    public async Task DeleteUriAlias_ChangesTypeAndTargetAsContentAdministrator_Ok()
    {
        var outdoorArea = new OutdoorAreaBuilder().Build();
        var uriAlias = new UriAliasBuilder("delete-me").SetType(UriType.OutdoorArea).SetOutdoorArea(outdoorArea).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAlias);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{uriAlias.Id}")
        {
            Content = GetJsonHttpBody(new DeleteUriAliasCommand { Version = uriAlias.Version })
        };

        var client = await CreateClientAsync(AuthorizationRoles.ContentAdmin);
        var response = await client
            .SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var persistedAliasExists = await BiBaBoulderDbContext.UriAliases.AnyAsync(
            item => item.Id == uriAlias.Id,
            TestContext.Current.CancellationToken);
        Assert.False(persistedAliasExists);
    }

    [Fact]
    public async Task GetUriAlias_AnonymousResolvesTarget_Ok()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        var uriAlias = new UriAliasBuilder("public-gym").SetType(UriType.BoulderGym).SetBoulderGym(boulderGym).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var response = await Client().GetAsync(
            $"/api/Medias/{uriAlias.Alias}/{(long)uriAlias.Type}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UriAliasDto>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.Equal(boulderGym.Id, result.Id);
    }

    private async Task<HttpClient> CreateClientAsync(string role)
    {
        var user = new UserBuilder().SetRoles(role).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        return AuthenticatedClient(userId: user.OidcSubject, role: role, username: user.Username);
    }
}
