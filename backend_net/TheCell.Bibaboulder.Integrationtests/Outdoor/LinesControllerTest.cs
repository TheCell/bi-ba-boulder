using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class LinesControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Lines";
    private readonly Faker _bogus;

    public LinesControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetLinesByBlocId_Anonymous_Ok()
    {
        var (bloc, lines) = await PrepareData();

        var response = await Client().GetAsync($"{_baseUrl}/by-bloc/{bloc.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LineDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(lines.Count, result.Count);

        foreach (var lineFromDb in lines)
        {
            var line = result.Single(l => l.Id == lineFromDb.Id);
            LineAssertion.Assert(line, lineFromDb);
        }
    }

    [Fact]
    public async Task GetLinesByBlocId_NoLines_Ok()
    {
        var response = await Client().GetAsync($"{_baseUrl}/by-bloc/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LineDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private async Task<(Bloc Bloc, List<Line> Lines)> PrepareData()
    {
        var sector = new SectorBuilder()
            .SetName(_bogus.Lorem.Slug())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAsync(sector);

        var bloc = new BlocBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetSectorId(sector.Id)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAsync(bloc);

        var lines = new List<Line>();
        for (var i = 0; i < 3; i++)
        {
            lines.Add(new LineBuilder()
                .SetIdentifier($"L-{i:D3}")
                .SetName(_bogus.Lorem.Slug())
                .SetColor("#FF0000")
                .SetBlocId(bloc.Id)
                .Build());
        }
        await BiBaBoulderDbContext.InsertEntitiesAsync(lines);

        return (bloc, lines);
    }
}
