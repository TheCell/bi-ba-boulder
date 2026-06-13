using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.BoulderLog;

[Collection(nameof(CollectionForIntegrationTests))]
public class BoulderLogsControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/BoulderLogs";

    public BoulderLogsControllerTest(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetBoulderLogs_Anonymous_Unauthorized()
    {
        var response = await Client().GetAsync(_baseUrl, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBoulderLogs_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var log = new BoulderLogBuilder().SetUser(user).SetIsSent(true).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(log);

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.GetAsync(_baseUrl, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<BoulderLogDto>>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result.Count >= 1);
        BoulderLogAssertion.Assert(log, result[0]);
    }

    [Fact]
    public async Task GetBoulderLog_Anonymous_Unauthorized()
    {
        var id = Guid.CreateVersion7();
        var response = await Client().GetAsync($"{_baseUrl}/{id}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBoulderLog_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var log = new BoulderLogBuilder().SetUser(user).SetIsSent(true).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(log);

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.GetAsync($"{_baseUrl}/{log.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderLogDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        BoulderLogAssertion.Assert(log, result);
    }

    [Fact]
    public async Task GetBoulderLogBySpraywall_Anonymous_Unauthorized()
    {
        var spraywallProblemId = Guid.CreateVersion7();
        var response = await Client().GetAsync($"{_baseUrl}/spraywall/{spraywallProblemId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBoulderLogBySpraywall_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var sprayWall = new SpraywallBuilder().Build();
        var spraywallProblem = new SpraywallProblemBuilder(user, sprayWall).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(spraywallProblem);

        var boulderLog = new BoulderLogBuilder()
            .SetUser(user)
            .SetIsSent(true)
            .SetIsProject(false)
            .SetRating(Rating.Four)
            .SetFontGrade(FontGrade.ThreePlus)
            .SetSpraywallProblem(spraywallProblem)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(boulderLog);

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.GetAsync($"{_baseUrl}/spraywall/{spraywallProblem.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderLogDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        BoulderLogAssertion.Assert(boulderLog, result);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task CreateBoulderLogForSpraywall_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);
        var sprayWall = new SpraywallBuilder().Build();
        var spraywallProblem = new SpraywallProblemBuilder(user, sprayWall).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(spraywallProblem);

        var command = new CreateBoulderLogCommand
        {
            IsSent = true,
            IsProject = false,
            Rating = Rating.Four,
            FontGrade = FontGrade.ThreePlus,
            SpraywallProblemId = spraywallProblem.Id
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.PutAsync($"{_baseUrl}/{spraywallProblem.Id}", GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderLogDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        BoulderLogAssertion.Assert(command, result);
        Assert.Equal(user.Id, result.UserId);
    }

    [Fact]
    public async Task UpdateBoulderLog_Anonymous_Unauthorized()
    {
        var id = Guid.CreateVersion7();
        var body = new UpdateBoulderLogCommand { Id = Guid.CreateVersion7(), Version = 1L, IsSent = true, IsProject = true, Rating = Rating.Five };
        var response = await Client().PostAsync($"{_baseUrl}/{id}", GetJsonHttpBody(body), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBoulderLog_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var log = new BoulderLogBuilder().SetUser(user).SetIsSent(false).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(log);

        var body = new UpdateBoulderLogCommand { Id = Guid.CreateVersion7(), Version = 1L, IsSent = true, IsProject = true, Rating = Rating.Five };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.PostAsync($"{_baseUrl}/{log.Id}", GetJsonHttpBody(body), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BoulderLogDto>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(result);
        Assert.True(result.IsSent);
        Assert.True(result.IsProject);
        Assert.Equal(Rating.Five, result.Rating);
    }

    [Fact]
    public async Task DeleteBoulderLog_Anonymous_Unauthorized()
    {
        var id = Guid.CreateVersion7();
        var body = new DeleteBoulderLogCommand { Id = id, Version = 1L };
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{id}")
        {
            Content = GetJsonHttpBody(body)
        };
        var response = await Client().SendAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBoulderLog_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var log = new BoulderLogBuilder().SetUser(user).Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(log);

        var body = new DeleteBoulderLogCommand { Id = Guid.CreateVersion7(), Version = 1L };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{log.Id}")
        {
            Content = GetJsonHttpBody(body)
        };
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
