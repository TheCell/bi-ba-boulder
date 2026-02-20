using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.BiBaBoulder.Authorization;
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

        var response = await PostRequestAsync("", command, authenticated: false);

        Assert.Equal(401, (int)response.StatusCode);
    }

    [Fact]
    public async Task CreateSector_Ok()
    {
        var command = new CreateSectorCommand
        {
            Id = Guid.NewGuid(),
            Name = "Test Sector",
            Description = "Integration test sector"
        };

        var response = await PostRequestAsync("", command, authenticated: true);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEqual(command.Id, result.Id);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
    }

    [Fact]
    public async Task GetSectors_Anonymous_Ok()
    {
        var sectors = await PrepareData();

        var response = await GetRequestAsync("", authenticated: false);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<SectorDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        foreach (var sectorFromDb in sectors)
        {
            var sector = result.Single(s => s.Id == sectorFromDb.Id);
            SectorAssertion.Assert(sector, sectorFromDb);
        }
    }

    [Fact]
    public async Task GetSectors_Ok()
    {
        var sectors = await PrepareData();

        var response = await GetRequestAsync("", authenticated: true);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<SectorDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        foreach (var sectorFromDb in sectors)
        {
            var sector = result.Single(s => s.Id == sectorFromDb.Id);
            SectorAssertion.Assert(sector, sectorFromDb);
        }
    }

    [Fact]
    public async Task GetSector_Ok()
    {
        var sectors = await PrepareData();

        var response = await GetRequestAsync($"/{sectors[0].Id}", authenticated: true);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        SectorAssertion.Assert(result, sectors[0]);
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
        await BiBaBoulderDbContext.InsertEntitiesAsync(sectors);
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

    protected async Task<HttpResponseMessage> PostRequestAsync(string route, object request, bool authenticated = false)
    {
        var client = authenticated ? AuthenticatedClient(role: AuthorizationRoles.Admin) : Client();
        return await client.PostAsync($"{_baseUrl}{route}", GetJsonHttpBody(request));
    }
}
