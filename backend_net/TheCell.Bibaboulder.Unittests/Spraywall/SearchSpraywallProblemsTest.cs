using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class SearchSpraywallProblemsTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserService;
    private readonly MockSpraywallImageService _imageService;
    private readonly Faker _bogus;

    public SearchSpraywallProblemsTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserService = new CurrentUserServiceMock();
        _imageService = new MockSpraywallImageService();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task SearchSpraywallProblems_EmptyResult_Ok()
    {
        var handler = new SearchSpraywallProblemsQueryHandler(_dbContext, _currentUserService, _imageService);
        var result = await handler.HandleAsync(new SearchSpraywallProblemsQuery
        {
            SpraywallId = Guid.CreateVersion7(),
            Page = 1
        });

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public async Task SearchSpraywallProblems_Ok()
    {
        var (spraywall, creators, spraywallProblems) = await PrepareProblems();
        AddMockImages(spraywall.Id, spraywallProblems);

        var handler = new SearchSpraywallProblemsQueryHandler(_dbContext, _currentUserService, _imageService);
        var result = await handler.HandleAsync(new SearchSpraywallProblemsQuery
        {
            SpraywallId = spraywall.Id,
            Page = 2
        });

        Assert.Equal(45, result.TotalCount);
        Assert.Equal(15, result.Problems.Count);
        var spraywallDto = result.Problems.First();
        var spraywallFromDb = spraywallProblems.Skip(30).First();
        SpraywallProblemAssertion.Assert(spraywallFromDb, spraywallDto);
        Assert.Equal("SpraywallProblem_30", spraywallDto.Name);
        Assert.Equal(creators.First().Username, spraywallDto.CreatedByName);
        Assert.False(spraywallDto.Metadata.CanDelete);
        Assert.False(spraywallDto.Metadata.CanEdit);
    }

    [Fact]
    public async Task SearchSpraywallProblems_AsCreator_Ok()
    {
        var creator = new UserBuilder().SetUsername("creator").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creator);

        var spraywall = new SpraywallBuilder().SetName("Wall").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var problem = new SpraywallProblemBuilder(creator, spraywall)
            .SetName("Problem A")
            .SetDescription(_bogus.Lorem.Paragraph())
            .SetFontGrade(FontGrade.Three)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);
        AddMockImage(spraywall.Id, problem.Id);

        _currentUserService.WithUser(creator);
        var handler = new SearchSpraywallProblemsQueryHandler(_dbContext, _currentUserService, _imageService);
        var result = await handler.HandleAsync(new SearchSpraywallProblemsQuery
        {
            SpraywallId = spraywall.Id,
            Page = 1
        });

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Problems);
        var spraywallDto = result.Problems.First();
        SpraywallProblemAssertion.Assert(problem, spraywallDto);
        Assert.Equal("Problem A", spraywallDto.Name);
        Assert.Equal("creator", spraywallDto.CreatedByName);
        Assert.True(spraywallDto.Metadata.CanDelete);
        Assert.True(spraywallDto.Metadata.CanEdit);
    }

    [Fact]
    public async Task SearchSpraywallProblems_AsAdmin_Ok()
    {
        var adminUser = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();

        var creator = new UserBuilder().SetUsername("creator").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creator);

        var spraywall = new SpraywallBuilder().SetName("Spraywall").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var problem = new SpraywallProblemBuilder(creator, spraywall)
            .SetName("Problem A")
            .SetDescription(_bogus.Lorem.Paragraph())
            .SetFontGrade(FontGrade.Three)
            .Build();
        AddMockImage(spraywall.Id, problem.Id);
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);

        _currentUserService.WithUser(adminUser);
        var handler = new SearchSpraywallProblemsQueryHandler(_dbContext, _currentUserService, _imageService);
        var result = await handler.HandleAsync(new SearchSpraywallProblemsQuery
        {
            SpraywallId = spraywall.Id,
            Page = 1
        });

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Problems);
        var spraywallDto = result.Problems.First();
        SpraywallProblemAssertion.Assert(problem, spraywallDto);
        Assert.Equal("Problem A", spraywallDto.Name);
        Assert.Equal(creator.Username, spraywallDto.CreatedByName);
        Assert.True(spraywallDto.Metadata.CanDelete);
        Assert.False(spraywallDto.Metadata.CanEdit);
    }

    [Fact]
    public async Task SearchSpraywallProblems_MissingImage_ThrowsIOException()
    {
        var creator = new UserBuilder().SetUsername("creator").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creator);

        var spraywall = new SpraywallBuilder().SetName("Wall").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var problem = new SpraywallProblemBuilder(creator, spraywall)
            .SetName("Problem A")
            .SetDescription(_bogus.Lorem.Paragraph())
            .SetFontGrade(FontGrade.Three)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);

        var handler = new SearchSpraywallProblemsQueryHandler(_dbContext, _currentUserService, _imageService);

        var exception = await Assert.ThrowsAsync<IOException>(() => handler.HandleAsync(new SearchSpraywallProblemsQuery
        {
            SpraywallId = spraywall.Id,
            Page = 1
        }));

        Assert.Contains(problem.Id.ToString(), exception.Message);
    }

    [Fact]
    public async Task SearchSpraywallProblems_FilterByGrade_FiltersCorrectly()
    {
        var (spraywall, users, spraywallProblems) = await PrepareProblems();
        AddMockImages(spraywall.Id, spraywallProblems);

        var handler = new SearchSpraywallProblemsQueryHandler(_dbContext, _currentUserService, _imageService);
        var result = await handler.HandleAsync(new SearchSpraywallProblemsQuery
        {
            SpraywallId = spraywall.Id,
            GradeMin = FontGrade.TwoMinus,
            Creator = users.First().Username,
            GradeMax = FontGrade.Four,
            DateOrder = "asc",
            Page = 1
        });

        Assert.Equal(4, result.TotalCount);
        var problem1 = result.Problems.First();
        Assert.Equal("SpraywallProblem_4", problem1.Name);
        Assert.Equal("Description_4", problem1.Description);
        var problem2 = result.Problems.Skip(1).First();
        Assert.Equal("SpraywallProblem_6", problem2.Name);
        Assert.Equal("Description_6", problem2.Description);
        var problem3 = result.Problems.Skip(2).First();
        Assert.Equal("SpraywallProblem_8", problem3.Name);
        Assert.Equal("Description_8", problem3.Description);
        var problem4 = result.Problems.Skip(3).First();
        Assert.Equal("SpraywallProblem_10", problem4.Name);
        Assert.Equal("Description_10", problem4.Description);
    }

    private async Task<(Thecell.Bibaboulder.Model.Model.Spraywall spraywall, List<User> users, List<SpraywallProblem> problems)> PrepareProblems()
    {
        var user1 = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user1);

        var user2 = new UserBuilder()
            .SetUsername(_bogus.Internet.UserName())
            .SetEmail(_bogus.Internet.Email())
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user2);

        var spraywall = new SpraywallBuilder()
            .SetName(_bogus.Lorem.Slug())
            .SetDescription(_bogus.Lorem.Sentence())
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var problems = new List<SpraywallProblem>();
        for (var i = 0; i < 45; i++)
        {
            var spraywallProblem = new SpraywallProblemBuilder(i % 2 == 0 ? user1 : user2, spraywall)
                .SetName($"SpraywallProblem_{i}")
                .SetDescription($"Description_{i}")
                .SetFontGrade((FontGrade)i)
                .Build();
            problems.Add(spraywallProblem);
        }
        await _dbContext.InsertEntitiesAndSaveChangesAsync(problems);

        return (spraywall, [user1, user2], problems);
    }

    private void AddMockImage(Guid spraywallId, Guid problemId)
    {
        var mockImageData = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        _imageService.SaveImageAsync(spraywallId, problemId, mockImageData).Wait();
    }

    private void AddMockImages(Guid spraywallId, List<SpraywallProblem> problems)
    {
        foreach (var problem in problems)
        {
            AddMockImage(spraywallId, problem.Id);
        }
    }
}
