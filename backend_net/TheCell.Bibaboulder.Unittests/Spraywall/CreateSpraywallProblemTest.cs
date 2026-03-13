using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class CreateSpraywallProblemTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Mock<ISpraywallImageService> _imageServiceMock;
    private readonly UserBuilder _testUser;

    private const string ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    private const string ValidBase64Png = $"data:image/png;base64,{ImageData}";

    public CreateSpraywallProblemTest()
    {
        _currentUserServiceMock = new CurrentUserServiceMock();
        _imageServiceMock = new Mock<ISpraywallImageService>();
        _imageServiceMock.Setup(imageServiceMock => imageServiceMock.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<byte[]>()))
            .Returns(Task.CompletedTask);
        _testUser = new UserBuilder();
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task CreateSpraywallProblem_NoUser_NotFoundException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);
        _testUser.SetRoles(AuthorizationRoles.Editor);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = (int)FontGrade.Three
        };
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("No current user configured in mock.", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_NotEditor_UnauthorizedAccessException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = (int)FontGrade.Three
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("User does not have the required role.", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_MissingImage_ArgumentException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);
        _testUser.SetRoles(AuthorizationRoles.Editor);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = "",
            FontGrade = (int)FontGrade.Three
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Image is required", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_NotBase64Image_ArgumentException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);
        _testUser.SetRoles(AuthorizationRoles.Editor);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
            FontGrade = (int)FontGrade.Three
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Image must be a valid base64 PNG string with data:image/png;base64, prefix", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_InvalidSpraywall_NotFoundException()
    {
        _testUser.SetRoles(AuthorizationRoles.Editor);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = Guid.NewGuid(),
            SpraywallId = guid,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = (int)FontGrade.Three
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal($"Spraywall not found. (Id: {guid})", ex.Message);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }

    [Fact]
    public async Task CreateSpraywallProblem_Ok()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        _testUser.SetRoles(AuthorizationRoles.Editor);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var guid = Guid.NewGuid();
        var command = new CreateSpraywallProblemCommand
        {
            Id = guid,
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = ValidBase64Png,
            FontGrade = (int)FontGrade.Three
        };
        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);
        await handler.HandleAsync(command);

        var problem = await _dbContext.SpraywallProblems.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        SpraywallAssertion.Assert(command, problem);

        Assert.NotEqual(guid, problem.Id);
        Assert.Equal(spraywall.Id, problem.SpraywallId);
        Assert.Equal(user.Id, problem.CreatorId);

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Once);
    }

    [Fact]
    public async Task CreateSpraywallProblem_InvalidImage_ThrowsArgumentException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);
        var user = _testUser.Build();
        await _dbContext.InsertEntityAsync(user);

        var command = new CreateSpraywallProblemCommand
        {
            SpraywallId = spraywall.Id,
            Name = "New Problem",
            Description = "A test problem",
            Image = "",
            FontGrade = (int)FontGrade.Three
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));

        _imageServiceMock.Verify(m => m.SaveImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), Convert.FromBase64String(ImageData)), Times.Never);
    }
}
