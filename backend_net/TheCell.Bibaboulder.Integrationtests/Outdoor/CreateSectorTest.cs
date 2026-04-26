using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class SectorsControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Sectors";
    private readonly Faker _bogus;

    public SectorsControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateSector_Anonymous_Unauthorized()
    {
        var command = new CreateSectorCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Sector",
            Description = "Integration test sector"
        };

        var client = Client();
        var response = await client.PostAsync(
            $"{_baseUrl}",
            GetJsonHttpBody(command),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSector_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateSectorCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Sector",
            Description = "Integration test sector"
        };

        var beforeSend = DateTime.UtcNow;
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.PostAsync(
            $"{_baseUrl}",
            GetJsonHttpBody(command),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);

        var sectorFromDb = await BiBaBoulderDbContext.Sectors
            .AsNoTracking()
            .SingleAsync(s => s.Id == result.Id, cancellationToken: TestContext.Current.CancellationToken);
        SectorAssertion.Assert(command, sectorFromDb);
        Assert.NotEqual(command.Id, result.Id);
    }

    [Fact]
    public async Task GetSectors_Anonymous_Ok()
    {
        var sectors = await PrepareData();

        var client = Client();
        var response = await client.GetAsync(
            $"{_baseUrl}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<SectorDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        foreach (var sectorFromDb in sectors)
        {
            var sector = result.Single(s => s.Id == sectorFromDb.Id);
            SectorAssertion.Assert(sectorFromDb, sector);
        }
    }

    [Fact]
    public async Task GetSector_Anonymous_Ok()
    {
        var sectors = await PrepareData();

        var client = Client();
        var response = await client.GetAsync(
            $"{_baseUrl}/{sectors[0].Id}",
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        SectorAssertion.Assert(sectors[0], result);
    }

    private async Task<List<Sector>> PrepareData()
    {
        var sectors = new List<Sector>();
        for (var i = 0; i < 5; i++)
        {
            sectors.Add(
                new SectorBuilder()
                .SetName(_bogus.Lorem.Slug())
                .SetDescription(_bogus.Lorem.Sentence())
                .Build());
        }
        await BiBaBoulderDbContext.InsertEntitiesAndSaveChangesAsync(sectors);
        return sectors;
    }

    //private async Task<HttpResponseMessage> PutRequestAsync(Guid id, object request)
    //{
    //    return await Client().PutAsJsonAsync($"{_baseUrl}/{id}", request);
    //}

    protected async Task<HttpResponseMessage> GetRequestAsync(string route, string query = "", bool authenticated = false)
    {
        var client = authenticated ? AuthenticatedClient(role: AuthorizationRoles.User) : Client();
        return await client.GetAsync($"{_baseUrl}{route}{query}");
    }

    //protected async Task<HttpResponseMessage> PostRequestAsync(string route, object request, bool authenticated = false)
    //{
    //    var client = authenticated ? AuthenticatedClient(role: AuthorizationRoles.Admin) : Client();
    //    return await client.PostAsync($"{_baseUrl}{route}", GetJsonHttpBody(request));
    //}
}
