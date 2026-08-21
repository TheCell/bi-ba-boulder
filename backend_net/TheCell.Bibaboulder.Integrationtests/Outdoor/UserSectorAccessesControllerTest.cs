using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class UserSectorAccessesControllerTest : BaseTest
{
    private const string BaseUrl = "/api/UserSectorAccesses";
    private readonly Faker _bogus = new("de_CH");

    public UserSectorAccessesControllerTest(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserSectorAccesses_AsUser_Forbidden()
    {
        var (user, _) = await CreateUserAndSectorAsync();
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);

        var response = await client.GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserSectorAccesses_AsAdmin_Ok()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);

        var response = await (await CreateAdminClientAsync()).GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var accesses = await response.Content.ReadFromJsonAsync<UserSectorAccessDto[]>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(accesses);
        Assert.Contains(accesses, result => result.UserId == user.Id && result.SectorId == sector.Id);
    }

    [Fact]
    public async Task GetUserSectorAccess_AsAdmin_Ok()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);

        var response = await (await CreateAdminClientAsync()).GetAsync($"{BaseUrl}/users/{user.Id}/sectors/{sector.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserSectorAccessDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(sector.Id, result.SectorId);
        Assert.Equal(access.AccessSourceType, result.AccessSourceType);
        Assert.Equal(access.ValidUntil, result.ValidUntil);
    }

    [Fact]
    public async Task GetUserSectorAccess_AsUser_Forbidden()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);

        var response = await client.GetAsync($"{BaseUrl}/users/{user.Id}/sectors/{sector.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateUserSectorAccess_AsAdmin_Ok()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var command = new CreateUserSectorAccessCommand
        {
            UserId = user.Id,
            SectorId = sector.Id,
            AccessSourceType = AccessSourceType.ManualGrant,
            ValidUntil = DateTime.UtcNow.AddDays(1)
        };

        var response = await (await CreateAdminClientAsync()).PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserSectorAccessDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(sector.Id, result.SectorId);
        Assert.Equal(command.AccessSourceType, result.AccessSourceType);
        Assert.Equal(command.ValidUntil, result.ValidUntil);
    }

    [Fact]
    public async Task CreateUserSectorAccess_AsUser_Forbidden()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var command = new CreateUserSectorAccessCommand
        {
            UserId = user.Id,
            SectorId = sector.Id,
            AccessSourceType = AccessSourceType.ManualGrant,
            ValidUntil = DateTime.UtcNow.AddDays(1)
        };
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);

        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserSectorAccess_AsAdmin_Ok()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);

        var updateCommand = new UpdateUserSectorAccessCommand
        {
            UserId = user.Id,
            SectorId = sector.Id,
            Version = access.Version,
            AccessSourceType = AccessSourceType.Subscription,
            ValidUntil = null
        };

        var response = await (await CreateAdminClientAsync()).PutAsync($"{BaseUrl}/users/{user.Id}/sectors/{sector.Id}", GetJsonHttpBody(updateCommand), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserSectorAccessDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(AccessSourceType.Subscription, result.AccessSourceType);
        Assert.Null(result.ValidUntil);
    }

    [Fact]
    public async Task UpdateUserSectorAccess_AsUser_Forbidden()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);
        var command = new UpdateUserSectorAccessCommand
        {
            UserId = user.Id,
            SectorId = sector.Id,
            Version = access.Version,
            AccessSourceType = AccessSourceType.Subscription,
            ValidUntil = null
        };
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);

        var response = await client.PutAsync($"{BaseUrl}/users/{user.Id}/sectors/{sector.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserSectorAccess_AsAdmin_Ok()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);

        var command = new DeleteUserSectorAccessCommand
        {
            UserId = user.Id,
            SectorId = sector.Id,
            Version = access.Version
        };
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/users/{user.Id}/sectors/{sector.Id}")
        {
            Content = GetJsonHttpBody(command)
        };
        var response = await (await CreateAdminClientAsync()).SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var exists = await BiBaBoulderDbContext.UserSectorAccesses
            .AnyAsync(item => item.UserId == user.Id && item.SectorId == sector.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteUserSectorAccess_AsUser_Forbidden()
    {
        var (user, sector) = await CreateUserAndSectorAsync();
        var access = new UserSectorAccessBuilder(user, sector).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(access);
        var command = new DeleteUserSectorAccessCommand
        {
            UserId = user.Id,
            SectorId = sector.Id,
            Version = access.Version
        };
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/users/{user.Id}/sectors/{sector.Id}")
        {
            Content = GetJsonHttpBody(command)
        };
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserSectorAccesses_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync(BaseUrl, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<(User User, Sector Sector)> CreateUserAndSectorAsync()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.User)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var sector = new SectorBuilder().SetName(_bogus.Lorem.Slug()).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        return (user, sector);
    }

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var admin = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(admin);

        return AuthenticatedClient(userId: admin.OidcSubject, role: AuthorizationRoles.Admin, username: admin.Username);
    }
}
