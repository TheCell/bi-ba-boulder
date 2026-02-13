using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
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
    private readonly string _baseUrl = "/api/sectors";
    private readonly Faker _bogus;
    // TODO anonymous and logged in test

    public SectorsControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
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

        var response = await PostRequestAsync(_baseUrl, command);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.NotEqual(command.Id, result.Id);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
    }

    [Fact]
    public async Task GetSector_Ok()
    {
        var sector = await PrepareData();

        var response = await GetRequestAsync($"/{sector.Id}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        SectorAssertion.Assert(result, sector);
    }

    private async Task<Sector> PrepareData()
    {
        var sector = new SectorBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetDescription(_bogus.Lorem.Sentence())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAsync(sector);
        return sector;
    }

    private async Task<HttpResponseMessage> PutRequestAsync(Guid id, object request)
    {
        return await Client().PutAsJsonAsync($"{_baseUrl}/{id}", request);
    }

    protected async Task<HttpResponseMessage> GetRequestAsync(string route, string query = "")
    {
        return await Client().GetAsync($"{_baseUrl}{route}{query}");
    }

    protected async Task<HttpResponseMessage> PostRequestAsync(string route, object request)
    {
        return await Client().PostAsync($"{_baseUrl}{route}", GetJsonHttpBody(request));
    }
}
