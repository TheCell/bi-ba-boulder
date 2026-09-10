using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class BlocsControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Blocs";
    private readonly Faker _bogus;

    public BlocsControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetBloc_NotFound_Returns404()
    {
        var response = await Client().GetAsync($"{_baseUrl}/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBloc_Anonymous_Ok()
    {
        var (_, blocs) = await PrepareData();
        var target = blocs[0];

        var response = await Client().GetAsync($"{_baseUrl}/{target.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BlocDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        BlocAssertion.Assert(target, result);
    }

    [Fact]
    public async Task GetBlocsBySectorId_Anonymous_Ok()
    {
        var (sector, blocs) = await PrepareData();

        var response = await Client().GetAsync($"{_baseUrl}/by-sector/{sector.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<BlocDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        foreach (var blocFromDb in blocs)
        {
            var bloc = result.Single(b => b.Id == blocFromDb.Id);
            BlocAssertion.Assert(blocFromDb, bloc);
        }
    }

    private async Task<(Sector Sector, List<Bloc> Blocs)> PrepareData()
    {
        var sector = new SectorBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetDescription(_bogus.Lorem.Sentence())
            .SetCoordinates("46.9906733, 7.5584747")
            .SetImportantInfo(_bogus.Lorem.Sentence())
            .SetIsPublic(true)
            .SetPreviewImageUri(_bogus.Internet.UrlWithPath())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var blocs = new List<Bloc>();
        for (var i = 0; i < 3; i++)
        {
            blocs.Add(new BlocBuilder()
                .SetName(_bogus.Lorem.Slug())
                .SetDescription(_bogus.Lorem.Sentence())
                .SetCoordinates("46.9914628, 7.5589870")
                .SetPreviewImageUri(_bogus.Internet.UrlWithPath())
                .SetSectorId(sector.Id)
                .SetAdditionalParts([
                    new BlocBuilder()
                        .SetName(_bogus.Lorem.Slug())
                        .SetDescription(_bogus.Lorem.Sentence())
                        .SetCoordinates("46.9921534, 7.5588492")
                        .SetBlocLowRes(_bogus.Internet.UrlWithPath())
                        .SetBlocMedRes(_bogus.Internet.UrlWithPath())
                        .SetBlocHighRes(_bogus.Internet.UrlWithPath())
                        .SetPreviewImageUri(_bogus.Internet.UrlWithPath())
                        .Build(),
                    new BlocBuilder()
                        .SetName(_bogus.Lorem.Slug())
                        .SetDescription(_bogus.Lorem.Sentence())
                        .SetCoordinates("46.9929293, 7.5582829")
                        .SetBlocLowRes(_bogus.Internet.UrlWithPath())
                        .SetBlocMedRes(_bogus.Internet.UrlWithPath())
                        .SetBlocHighRes(_bogus.Internet.UrlWithPath())
                        .Build(),
                    new BlocBuilder()
                        .SetName(_bogus.Lorem.Slug())
                        .SetDescription(_bogus.Lorem.Sentence())
                        .SetCoordinates("46.9914628, 7.5589870")
                        .SetBlocLowRes(_bogus.Internet.UrlWithPath())
                        .SetBlocMedRes(_bogus.Internet.UrlWithPath())
                        .SetBlocHighRes(_bogus.Internet.UrlWithPath())
                        .Build()])
                .Build());
        }
        await BiBaBoulderDbContext.InsertEntitiesAndSaveChangesAsync(blocs);

        return (sector, blocs);
    }
}
