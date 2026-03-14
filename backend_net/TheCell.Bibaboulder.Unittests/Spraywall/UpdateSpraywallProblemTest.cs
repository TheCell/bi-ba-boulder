using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thecell.Bibaboulder.BiBaBoulder.Services;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class UpdateSpraywallProblemTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly MockSpraywallImageService _spraywallImageServiceMock;
    private readonly Faker _bogus;

    private const string ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    private const string UpdateImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
    private const string ValidUpdateBase64Png = $"data:image/png;base64,{UpdateImageData}";

    public UpdateSpraywallProblemTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _spraywallImageServiceMock = new MockSpraywallImageService();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task UpdateSpraywallProblem_MissingProblem_NotFoundException()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var guid = Guid.NewGuid();
        var command = new UpdateSpraywallProblemCommand
        {
            Id = guid,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = ValidUpdateBase64Png,
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(editor);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal($"SpraywallProblem not found. (Id: {guid})", ex.Message);
    }

    [Fact]
    public async Task UpdateSpraywallProblem_NotCreator_AccessDeniedException()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var otherUser = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(otherUser);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var command = new UpdateSpraywallProblemCommand
        {
            Id = problem.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = ValidUpdateBase64Png,
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(otherUser);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Only the creator can update this problem", ex.Message);
    }

    [Fact]
    public async Task UpdateSpraywallProblem_Admin_AccessDeniedException()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var otherUser = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await _dbContext.InsertEntityAsync(otherUser);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var command = new UpdateSpraywallProblemCommand
        {
            Id = problem.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = ValidUpdateBase64Png,
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(otherUser);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Only the creator can update this problem", ex.Message);
    }

    [Fact]
    public async Task UpdateSpraywallProblem_NoEditorRole_AccessDeniedException()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var command = new UpdateSpraywallProblemCommand
        {
            Id = problem.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = ValidUpdateBase64Png,
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(editor);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Only users with editor role or higher can edit problems", ex.Message);
    }

    [Fact]
    public async Task UpdateSpraywallProblem_ImageData_ArgumentException()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var command = new UpdateSpraywallProblemCommand
        {
            Id = problem.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = "",
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(editor);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Image is required", ex.Message);
    }

    [Fact]
    public async Task UpdateSpraywallProblem_MalformedImageData_ArgumentException()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var command = new UpdateSpraywallProblemCommand
        {
            Id = problem.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = UpdateImageData,
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(editor);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Image must be a valid base64 PNG string with data:image/png;base64, prefix", ex.Message);
    }

    [Fact]
    public async Task UpdateSpraywallProblem_Ok()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Original Name")
            .SetDescription("Original Description")
            .SetFontGrade(FontGrade.EightB)
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        await _spraywallImageServiceMock.SaveImageAsync(problem.SpraywallId, problem.Id, Convert.FromBase64String(ImageData));

        var command = new UpdateSpraywallProblemCommand
        {
            Id = problem.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Sentence(),
            Image = ValidUpdateBase64Png,
            FontGrade = (int)_bogus.Random.Enum<FontGrade>(),
            Version = problem.Version
        };

        _currentUserServiceMock.WithUser(editor);
        var handler = new UpdateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock);
        await handler.HandleAsync(command);

        var updated = await _dbContext.SpraywallProblems
            .AsNoTracking()
            .SingleAsync(sp => sp.Id == problem.Id, cancellationToken: TestContext.Current.CancellationToken);
        SpraywallProblemAssertion.Assert(command, updated);

        var updatedImage = await _spraywallImageServiceMock.GetImageAsBase64Async(updated.SpraywallId, updated.Id);
        Assert.Equal(command.Image, updatedImage);
    }
}
