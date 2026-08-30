using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

[Collection(nameof(CollectionForIntegrationTests))]
public class SectorAdministrationControllerTest : BaseTest
{
    private const string BaseUrl = "/api/Sectors";
    private readonly Faker _bogus;

    public SectorAdministrationControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task UpdateSector_Anonymous_Unauthorized()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new UpdateSectorCommand { Name = _bogus.Lorem.Slug(), Version = sector.Version };

        var response = await Client().PutAsync($"{BaseUrl}/{sector.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSector_WithOutdoorAreas_Ok()
    {
        var sector = new SectorBuilder()
            .SetName("Original")
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Lindental")
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new UpdateSectorCommand
        {
            Name = _bogus.Lorem.Slug(),
            Coordinates = "46.9914628, 7.5589870",
            IsPublic = true,
            Version = sector.Version,
            OutdoorAreaIds = [outdoorArea.Id]
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.ContentAdmin);
        var response = await client.PutAsync($"{BaseUrl}/{sector.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SectorDto>(cancellationToken: TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.True(result.IsPublic);
        Assert.Single(result.OutdoorAreas);
        Assert.Equal(outdoorArea.Id, result.OutdoorAreas.Single().Id);
        Assert.Equal(sector.Version + 1, result.Version);
    }

    [Fact]
    public async Task UpdateSector_UnknownOutdoorArea_BadRequest()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new UpdateSectorCommand
        {
            Name = _bogus.Lorem.Slug(),
            Version = sector.Version,
            OutdoorAreaIds = [System.Guid.NewGuid()]
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.PutAsync($"{BaseUrl}/{sector.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSector_Ok()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{sector.Id}")
        {
            Content = GetJsonHttpBody(new DeleteSectorCommand { Version = sector.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.Admin);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var exists = await BiBaBoulderDbContext.Sectors.AnyAsync(item => item.Id == sector.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteSector_User_Forbidden()
    {
        var sector = new SectorBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{sector.Id}")
        {
            Content = GetJsonHttpBody(new DeleteSectorCommand { Version = sector.Version })
        };

        var client = AuthenticatedClient(role: AuthorizationRoles.User);
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
