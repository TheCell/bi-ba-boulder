using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class BlocAdministrationControllerTest : BaseTest
{
    private const string BaseUrl = "/api/Blocs";
    private readonly Faker _bogus;

    public BlocAdministrationControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateBloc_Anonymous_Unauthorized()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new CreateBlocCommand { SectorId = sector.Id, Name = _bogus.Lorem.Slug() };

        var response = await Client().PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBloc_User_Forbidden()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new CreateBlocCommand { SectorId = sector.Id, Name = _bogus.Lorem.Slug() };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBloc_ContentAdmin_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new CreateBlocCommand
        {
            SectorId = sector.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BlocDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);

        var bloc = await BiBaBoulderDbContext.Blocs
            .AsNoTracking()
            .Include(item => item.AdditionalParts)
            .SingleAsync(item => item.Id == result.Id, TestContext.Current.CancellationToken);
        command.Id = bloc.Id;
        BlocAssertion.Assert(command, bloc);
        BlocAssertion.Assert(bloc, result);
    }

    [Fact]
    public async Task CreateBloc_UnknownSector_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var command = new CreateBlocCommand
        {
            SectorId = Guid.CreateVersion7(),
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task CreateBloc_UnknownSector_NotFound()
    {
        var command = new CreateBlocCommand { SectorId = Guid.CreateVersion7(), Name = _bogus.Lorem.Slug() };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.PostAsync(BaseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBloc_Anonymous_Unauthorized()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand { SectorId = sector.Id, Name = _bogus.Lorem.Slug(), Version = bloc.Version };

        var response = await Client().PutAsync($"{BaseUrl}/{bloc.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBloc_Admin_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = contentAdmin.Id;
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand
        {
            SectorId = sector.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Coordinates = "46.9906733, 7.5584747",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath(),
            Version = bloc.Version
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PutAsync($"{BaseUrl}/{bloc.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BlocDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);

        var updated = await BiBaBoulderDbContext.Blocs
            .AsNoTracking()
            .Include(item => item.AdditionalParts)
            .SingleAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        command.Id = bloc.Id;
        BlocAssertion.Assert(command, updated);
        BlocAssertion.Assert(updated, result);
    }

    [Fact]
    public async Task UpdateBloc_OutdatedVersion_Conflict()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = contentAdmin.Id;
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand { SectorId = sector.Id, Name = _bogus.Lorem.Slug(), Version = bloc.Version + 1 };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.PutAsync($"{BaseUrl}/{bloc.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBloc_Admin_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(contentAdmin);

        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = contentAdmin.Id;
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{bloc.Id}")
        {
            Content = GetJsonHttpBody(new DeleteBlocCommand { Version = bloc.Version })
        };

        var client = AuthenticatedClient(userId: contentAdmin.OidcSubject, role: AuthorizationRoles.ContentAdmin, username: contentAdmin.Username);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var exists = await BiBaBoulderDbContext.Blocs.AnyAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteBloc_User_Forbidden()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{bloc.Id}")
        {
            Content = GetJsonHttpBody(new DeleteBlocCommand { Version = bloc.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
