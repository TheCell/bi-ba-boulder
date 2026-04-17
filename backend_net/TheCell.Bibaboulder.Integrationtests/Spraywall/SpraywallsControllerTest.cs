using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class SpraywallsControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Spraywalls";
    private readonly Faker _bogus;

    public SpraywallsControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task GetSpraywalls_Anonymous_Ok()
    {
        var spraywalls = await PrepareSpraywalls();

        var response = await Client().GetAsync(_baseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<SpraywallDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        foreach (var spraywall in spraywalls)
        {
            var sw = result.Single(s => s.Id == spraywall.Id);
            SpraywallAssertion.Assert(spraywall, sw);
        }
    }

    [Fact]
    public async Task SearchSpraywallProblems_Anonymous_Ok()
    {
        var (spraywall, _, _) = await PrepareProblems();

        var response = await Client().PostAsync(
            $"{_baseUrl}/{spraywall.Id}/problems",
            GetJsonHttpBody(new { Page = 1 }),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SpraywallProblemListDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task CreateSpraywallProblem_Anonymous_Unauthorized()
    {
        var spraywalls = await PrepareSpraywalls();

        var body = new
        {
            Name = "Test Problem",
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
        };

        var response = await Client().PutAsync(
            $"{_baseUrl}/{spraywalls[0].Id}/problem",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateSpraywallProblem_Editor_Ok()
    {
        var (spraywall, user, _) = await PrepareProblems();

        var body = new CreateSpraywallProblemCommand
        {
            Name = "New Problem",
            Description = "Created via integration test",
            Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            FontGrade = FontGrade.TwoMinus
        };

        var beforeSend = DateTime.UtcNow;
        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Editor, username: user.Username);
        var response = await client.PutAsync(
            $"{_baseUrl}/{spraywall.Id}/problem",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SpraywallProblemDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        SpraywallProblemAssertion.Assert(body, result);
        Assert.Equal(user.Id, result.CreatedById);
        Assert.Equal(user.Username, result.CreatedByName);
        Assert.True(DateTime.Parse(result.CreatedDate) >= beforeSend);
        Assert.True(result.Metadata.CanDelete);
        Assert.True(result.Metadata.CanEdit);
    }

    private async Task<List<Thecell.Bibaboulder.Model.Model.Spraywall>> PrepareSpraywalls()
    {
        var spraywalls = new List<Thecell.Bibaboulder.Model.Model.Spraywall>();
        for (var i = 0; i < 3; i++)
        {
            spraywalls.Add(new SpraywallBuilder()
                .SetName(_bogus.Lorem.Slug())
                .SetDescription(_bogus.Lorem.Sentence())
                .Build());
        }
        await BiBaBoulderDbContext.InsertEntitiesAsync(spraywalls);
        return spraywalls;
    }

    private async Task<(Thecell.Bibaboulder.Model.Model.Spraywall Spraywall, User User, List<SpraywallProblem> Problems)> PrepareProblems()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAsync(user);

        var spraywall = new SpraywallBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetDescription(_bogus.Lorem.Sentence())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAsync(spraywall);

        var problems = new List<SpraywallProblem>();
        for (var i = 0; i < 15; i++)
        {
            var spraywallProblem = new SpraywallProblemBuilder(user, spraywall)
                .SetName($"Spraywall_{i}")
                .SetDescription($"Description_{i}")
                .SetFontGrade((FontGrade)i)
                .Build();
            problems.Add(spraywallProblem);
        }
        await BiBaBoulderDbContext.InsertEntitiesAsync(problems);

        return (spraywall, user, problems);
    }
}
