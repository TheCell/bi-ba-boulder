using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class UsersControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Users";
    private readonly Faker _bogus;

    public UsersControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetSelf_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync($"{_baseUrl}/me", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSelf_Ok()
    {
        var user = await PrepareUser();

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: AuthorizationRoles.User,
            username: user.Username);

        var response = await client.GetAsync($"{_baseUrl}/me", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        UserAssertion.Assert(user, result);
    }

    [Fact]
    public async Task GetUserById_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync($"{_baseUrl}/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_Ok()
    {
        var targetUser = await PrepareUser();

        var adminUser = new UserBuilder()
            .SetEmail("admin@test.com")
            .SetUsername("admin")
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(adminUser);

        var client = AuthenticatedClient(
            userId: adminUser.OidcSubject,
            role: AuthorizationRoles.Admin,
            username: adminUser.Username);

        var response = await client.GetAsync($"{_baseUrl}/{targetUser.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UserDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(targetUser.Id, result.Id);
    }

    [Fact]
    public async Task GetUserById_NonAdmin_Forbidden()
    {
        var user = await PrepareUser();

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: AuthorizationRoles.User,
            username: user.Username);

        var response = await client.GetAsync($"{_baseUrl}/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<User> PrepareUser()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);
        return user;
    }
}
