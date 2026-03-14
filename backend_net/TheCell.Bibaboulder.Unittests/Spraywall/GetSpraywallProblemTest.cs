using System;
using System.Threading.Tasks;
using Moq;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class GetSpraywallProblemTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Mock<ISpraywallImageService> _spraywallImageServiceMock;

    private const string ImageData = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";
    private const string ValidBase64Png = $"data:image/png;base64,{ImageData}";

    public GetSpraywallProblemTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _spraywallImageServiceMock = new Mock<ISpraywallImageService>();
        _spraywallImageServiceMock.Setup(spraywallImageService => spraywallImageService.GetImageAsBase64Async(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(ValidBase64Png);
    }

    [Fact]
    public async Task GetSpraywallProblem_NotFoundException()
    {
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetSpraywallProblemQuery
            {
                Id = Guid.CreateVersion7()
            }));
    }

    [Fact]
    public async Task GetSpraywallProblem_MissingImage()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Test Problem")
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        _currentUserServiceMock.WithUser(editor);
        _spraywallImageServiceMock.Reset();
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock.Object);
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetSpraywallProblemQuery
            {
                Id = problem.Id
            }));
        Assert.Equal($"Image for spraywall problem with id {problem.Id} not found.", ex.Message);
    }

    [Fact]
    public async Task GetSpraywallProblem_CreatorWithoutRole_Ok()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Test Problem")
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        _currentUserServiceMock.WithUser(editor);
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock.Object);
        var result = await handler.HandleAsync(new GetSpraywallProblemQuery
        {
            Id = problem.Id
        });

        SpraywallProblemAssertion.Assert(problem, result);
        Assert.Equal(editor.Username, result.CreatedByName);
        Assert.Equal(ValidBase64Png, result.Image);
        Assert.False(result.Metadata.CanEdit);
        Assert.False(result.Metadata.CanDelete);

        _spraywallImageServiceMock.Verify(spraywallImageService => spraywallImageService.GetImageAsBase64Async(spraywall.Id, problem.Id), Times.Once);
    }

    [Fact]
    public async Task GetSpraywallProblem_Creator_Ok()
    {
        var editor = new UserBuilder()
            .SetUsername("creator")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Test Problem")
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        _currentUserServiceMock.WithUser(editor);
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock.Object);
        var result = await handler.HandleAsync(new GetSpraywallProblemQuery
        {
            Id = problem.Id
        });

        SpraywallProblemAssertion.Assert(problem, result);
        Assert.Equal(editor.Username, result.CreatedByName);
        Assert.Equal(ValidBase64Png, result.Image);
        Assert.True(result.Metadata.CanEdit);
        Assert.True(result.Metadata.CanDelete);

        _spraywallImageServiceMock.Verify(spraywallImageService => spraywallImageService.GetImageAsBase64Async(spraywall.Id, problem.Id), Times.Once);
    }

    [Fact]
    public async Task GetSpraywallProblem_Admin_Ok()
    {
        var editor = new UserBuilder()
            .SetUsername("editor")
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAsync(editor);

        var admin = new UserBuilder()
            .SetUsername("admin")
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await _dbContext.InsertEntityAsync(admin);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAsync(spraywall);

        var problem = new SpraywallProblemBuilder(editor, spraywall)
            .SetName("Test Problem")
            .Build();
        await _dbContext.InsertEntityAsync(problem);

        _currentUserServiceMock.WithUser(admin);
        var handler = new GetSpraywallProblemQueryHandler(_dbContext, _currentUserServiceMock, _spraywallImageServiceMock.Object);
        var result = await handler.HandleAsync(new GetSpraywallProblemQuery
        {
            Id = problem.Id
        });

        SpraywallProblemAssertion.Assert(problem, result);
        Assert.Equal(editor.Username, result.CreatedByName);
        Assert.Equal(ValidBase64Png, result.Image);
        Assert.False(result.Metadata.CanEdit);
        Assert.True(result.Metadata.CanDelete);

        _spraywallImageServiceMock.Verify(spraywallImageService => spraywallImageService.GetImageAsBase64Async(spraywall.Id, problem.Id), Times.Once);
    }
}
