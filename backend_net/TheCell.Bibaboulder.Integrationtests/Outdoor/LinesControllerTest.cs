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
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
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
    public async Task GetLinesByBlocId_Anonymous_Unauthorized()
    {
        var (bloc, _) = await PrepareData();

        var response = await Client().GetAsync($"{_baseUrl}/by-bloc/{bloc.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLinesByBlocId_NormalUser_Forbidden()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.User)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var (bloc, _) = await PrepareData();

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.GetAsync($"{_baseUrl}/by-bloc/{bloc.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetLinesByBlocId_Admin_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var (bloc, lines) = await PrepareData();

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.GetAsync($"{_baseUrl}/by-bloc/{bloc.Id}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LineDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(lines.Count, result.Count);

        foreach (var lineFromDb in lines)
        {
            var line = result.Single(l => l.Id == lineFromDb.Id);
            LineAssertion.Assert(lineFromDb, line);
        }
    }

    [Fact]
    public async Task GetLinesByBlocId_NoLines_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.GetAsync($"{_baseUrl}/by-bloc/{Guid.CreateVersion7()}", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<LineDto>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateLine_Anonymous_Unauthorized()
    {
        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            Id = Guid.NewGuid(),
            BlocId = bloc.Id,
            Identifier = "L-100",
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            FontGrade = FontGrade.Four,
            Data = CreateLineData()
        };

        var response = await Client().PostAsync(_baseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateLine_User_Forbidden()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.User)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            BlocId = bloc.Id,
            Identifier = "L-101",
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            FontGrade = FontGrade.Four,
            Data = CreateLineData()
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.PostAsync(_baseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateLine_Admin_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            Id = Guid.NewGuid(),
            BlocId = bloc.Id,
            Identifier = "L-102",
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            FontGrade = FontGrade.Four,
            Data = CreateLineData()
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.PostAsync(_baseUrl, GetJsonHttpBody(command), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LineDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var lineFromDb = await BiBaBoulderDbContext.Lines
            .AsNoTracking()
            .SingleAsync(l => l.Id == result.Id, cancellationToken: TestContext.Current.CancellationToken);
        LineAssertion.Assert(lineFromDb, result);
        Assert.Equal(bloc.Id, lineFromDb.BlocId);
    }

    [Fact]
    public async Task UpdateLine_Anonymous_Unauthorized()
    {
        var id = Guid.CreateVersion7();
        var body = new UpdateLineCommand
        {
            Version = 1,
            Identifier = "L-200",
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            FontGrade = FontGrade.FourPlus,
            Data = CreateLineData()
        };

        var response = await Client().PostAsync($"{_baseUrl}/{id}", GetJsonHttpBody(body), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLine_User_Forbidden()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.User)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var line = await PrepareLine();

        var body = new UpdateLineCommand
        {
            Version = line.Version,
            Identifier = "L-201",
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            FontGrade = FontGrade.FourPlus,
            Data = CreateLineData()
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var response = await client.PostAsync($"{_baseUrl}/{line.Id}", GetJsonHttpBody(body), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLine_Admin_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var line = await PrepareLine();

        var body = new UpdateLineCommand
        {
            Version = line.Version,
            Identifier = "L-202",
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            FontGrade = FontGrade.Five,
            Data = CreateLineData()
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var response = await client.PostAsync($"{_baseUrl}/{line.Id}", GetJsonHttpBody(body), TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LineDto>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        var lineFromDb = await BiBaBoulderDbContext.Lines
            .AsNoTracking()
            .SingleAsync(l => l.Id == line.Id, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(body.Identifier, lineFromDb.Identifier);
        Assert.Equal(body.Name, lineFromDb.Name);
        Assert.Equal(body.Description, lineFromDb.Description);
        Assert.Equal(body.FontGrade, lineFromDb.FontGrade);
        LineAssertion.Assert(lineFromDb, result);
        Assert.Equal(2, lineFromDb.Version);
    }

    [Fact]
    public async Task DeleteLine_Anonymous_Unauthorized()
    {
        var id = Guid.CreateVersion7();
        var body = new DeleteLineCommand
        {
            Version = 1
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{id}")
        {
            Content = GetJsonHttpBody(body)
        };

        var response = await Client().SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLine_User_Forbidden()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.User)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var line = await PrepareLine();

        var body = new DeleteLineCommand
        {
            Version = line.Version
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.User, username: user.Username);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{line.Id}")
        {
            Content = GetJsonHttpBody(body)
        };

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLine_Admin_Ok()
    {
        var user = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var line = await PrepareLine();

        var body = new DeleteLineCommand
        {
            Version = line.Version
        };

        var client = AuthenticatedClient(userId: user.OidcSubject, role: AuthorizationRoles.Admin, username: user.Username);
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}/{line.Id}")
        {
            Content = GetJsonHttpBody(body)
        };

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        var exists = await BiBaBoulderDbContext.Lines.AnyAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    private async Task<(Bloc Bloc, List<Line> Lines)> PrepareData()
    {
        var sector = new SectorBuilder()
            .SetName(_bogus.Lorem.Slug())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetSectorId(sector.Id)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        var lines = new List<Line>();
        var lineData = new LineData
        {
            Positions =
            [
                [3.195283951339383, 2.7121955460871323, -3.141968702184311],
                [2.7787678080631806, 3.0940820944214265, -2.682641271014257],
                [1.977430016520973, 3.403700634971949, -2.5212089785318055],
                [0.7666454617614173, 3.681687464013387, -2.3818453599786906],
                [-0.8181475999640135, 4.387868646064916, -1.8669995023170851],
                [-0.3406801199316895, 5.988204120956535, -0.8902311027259602],
                [1.91972164868996, 6.697299204986229, 0.201622173490428]
            ]
        };

        for (var i = 0; i < 3; i++)
        {
            lines.Add(new LineBuilder()
                .SetIdentifier($"L-{i:D3}")
                .SetName(_bogus.Lorem.Slug())
                .SetDescription(_bogus.Lorem.Slug())
                .SetData(lineData)
                .SetFontGrade(_bogus.PickRandom<FontGrade>())
                .SetBlocId(bloc.Id)
                .Build());
        }
        await BiBaBoulderDbContext.InsertEntitiesAndSaveChangesAsync(lines);

        return (bloc, lines);
    }

    private async Task<Bloc> PrepareBloc()
    {
        var sector = new SectorBuilder()
            .SetName(_bogus.Lorem.Slug())
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetSectorId(sector.Id)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(bloc);

        return bloc;
    }

    private async Task<Line> PrepareLine()
    {
        var bloc = await PrepareBloc();

        var line = new LineBuilder()
            .SetIdentifier("L-001")
            .SetName(_bogus.Lorem.Slug())
            .SetDescription(_bogus.Lorem.Sentence())
            .SetFontGrade(FontGrade.ThreePlus)
            .SetData(CreateLineData())
            .SetBlocId(bloc.Id)
            .Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(line);

        return line;
    }

    private static LineData CreateLineData()
    {
        return new LineData
        {
            Positions =
            [
                [3.195283951339383, 2.7121955460871323, -3.141968702184311],
                [2.7787678080631806, 3.0940820944214265, -2.682641271014257],
                [1.977430016520973, 3.403700634971949, -2.5212089785318055],
                [0.7666454617614173, 3.681687464013387, -2.3818453599786906],
                [-0.8181475999640135, 4.387868646064916, -1.8669995023170851],
                [-0.3406801199316895, 5.988204120956535, -0.8902311027259602],
                [1.91972164868996, 6.697299204986229, 0.201622173490428]
            ]
        };
    }
}
