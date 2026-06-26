using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class SpraywallProblemsControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/SpraywallProblems";
    private readonly Faker _bogus;

    private const string ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    private const string UpdateImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
    private const string ValidBase64Png = $"data:image/png;base64,{ImageData}";

    public SpraywallProblemsControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetProblem_Anonymous_Ok()
    {
        var (user, problem) = await PrepareData();

        var response = await Client().GetAsync($"{_baseUrl}/{problem.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SpraywallProblemDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        SpraywallProblemAssertion.Assert(problem, result);
        Assert.Equal(user.Username, result.CreatedByName);
        Assert.Equal(ValidBase64Png, result.Image);
        Assert.False(result.Metadata.CanDelete);
        Assert.False(result.Metadata.CanEdit);
    }

    [Fact]
    public async Task UpdateProblem_Anonymous_Unauthorized()
    {
        var (_, problem) = await PrepareData();

        var body = new UpdateSpraywallProblemCommand
        {
            Name = "Updated",
            Image = $"data:image/png;base64,{UpdateImageData}",
            Version = problem.Version
        };

        var response = await Client().PostAsync(
            $"{_baseUrl}/{problem.Id}",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProblem_Ok()
    {
        var (user, problem) = await PrepareData();

        var command = new UpdateSpraywallProblemCommand
        {
            Name = "Updated",
            Image = $"data:image/png;base64,{UpdateImageData}",
            Version = problem.Version
        };

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: $"{AuthorizationRoles.Editor}",
            username: user.Username);

        var response = await client.PostAsync(
            $"{_baseUrl}/{problem.Id}",
            GetJsonHttpBody(command),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var dbProblem = await BiBaBoulderDbContext.SpraywallProblems
            .AsNoTracking()
            .SingleAsync(sp => sp.Id == problem.Id, TestContext.Current.CancellationToken);
        SpraywallProblemAssertion.Assert(command, dbProblem);
        Assert.Equal(problem.Id, dbProblem.Id);
        Assert.Equal(problem.Version + 1, dbProblem.Version);
        var updatedImage = await SpraywallImageService.GetImageAsBase64Async(problem.SpraywallId, problem.Id);
        Assert.Equal(command.Image, updatedImage);
    }

    [Fact]
    public async Task DeleteProblem_Anonymous_Unauthorized()
    {
        var (_, problem) = await PrepareData();

        var response = await Client().DeleteAsync($"{_baseUrl}/{problem.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProblem_Ok()
    {
        var (user, problem) = await PrepareData();

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: $"{AuthorizationRoles.Editor}",
            username: user.Username);

        var response = await client.DeleteAsync($"{_baseUrl}/{problem.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var spraywall = BiBaBoulderDbContext.SpraywallProblems
            .AsNoTracking()
            .SingleOrDefault(sp => sp.Id == problem.Id);
        Assert.Null(spraywall);
        var deletedImage = await SpraywallImageService.GetImageAsBase64Async(problem.SpraywallId, problem.Id);
        Assert.Null(deletedImage);
    }

    private async Task<(User User, SpraywallProblem Problem)> PrepareData()
    {
        var user = new UserBuilder().SetUsername(_bogus.Internet.UserName()).SetRoles(AuthorizationRoles.Editor).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var spraywall = new SpraywallBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var problem = new SpraywallProblemBuilder(user, spraywall)
            .SetCreator(user)
            .SetIsCircuit(_bogus.Random.Bool())
            .SetNoMatch(_bogus.Random.Bool())
            .SetFreeFeet(_bogus.Random.Bool())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(problem);

        await SpraywallImageService.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        return (user, problem);
    }
}
