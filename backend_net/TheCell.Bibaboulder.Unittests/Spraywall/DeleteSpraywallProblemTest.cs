using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class DeleteSpraywallProblemTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Mock<ISpraywallImageService> _imageServiceMock;

    public DeleteSpraywallProblemTest()
    {
        _currentUserServiceMock = new CurrentUserServiceMock();
        _imageServiceMock = new Mock<ISpraywallImageService>();
        _imageServiceMock.Setup(_imageServiceMock => _imageServiceMock.DeleteImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task DeleteSpraywall_MissingSpraywallProblem_NotFoundException()
    {
        var creatorUser = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .SetEmail("creator@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creatorUser);

        _currentUserServiceMock.WithUser(creatorUser);
        var handler = new DeleteSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new DeleteSpraywallProblemCommand { Id = Guid.CreateVersion7() }));
    }

    [Fact]
    public async Task DeleteSpraywall_NoEditRights_AccessDenied()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var creatorUser = new UserBuilder()
            .SetEmail("creator@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creatorUser);

        var problem = new SpraywallProblemBuilder(creatorUser, spraywall)
            .SetName("Try to delete me")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);

        _currentUserServiceMock.WithUser(creatorUser);
        var handler = new DeleteSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            await handler.HandleAsync(new DeleteSpraywallProblemCommand { Id = problem.Id }));
        Assert.Equal("Only users with editor role or higher can delete problems", ex.Message);

        _imageServiceMock.Verify(imageService => imageService.DeleteImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSpraywall_ByOtherEditor_AccessDeniedException()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var creatorUser = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .SetEmail("creator@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creatorUser);

        var otherUser = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .SetEmail("other@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(otherUser);

        var problem = new SpraywallProblemBuilder(creatorUser, spraywall)
            .SetName("To Delete")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);

        _currentUserServiceMock.WithUser(otherUser);
        var handler = new DeleteSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            await handler.HandleAsync(new DeleteSpraywallProblemCommand { Id = problem.Id }));
        Assert.Equal("Only the creator or an admin can delete this problem", ex.Message);

        _imageServiceMock.Verify(imageService => imageService.DeleteImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteSpraywall_ByCreator_Ok()
    {
        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var creatorUser = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .SetEmail("creator@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creatorUser);

        var problem = new SpraywallProblemBuilder(creatorUser, spraywall)
            .SetName("To Delete")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);

        _currentUserServiceMock.WithUser(creatorUser);
        var handler = new DeleteSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);
        await handler.HandleAsync(new DeleteSpraywallProblemCommand { Id = problem.Id });

        var exists = await _dbContext.SpraywallProblems.AnyAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(exists);

        _imageServiceMock.Verify(imageService => imageService.DeleteImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSpraywall_ByAdmin_Ok()
    {
        var adminUser = new UserBuilder()
            .SetRoles(AuthorizationRoles.Admin)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(adminUser);

        var creatorUser = new UserBuilder()
            .SetRoles(AuthorizationRoles.Admin)
            .SetEmail("creator@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creatorUser);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var problem = new SpraywallProblemBuilder(creatorUser, spraywall)
            .SetName("Others Problem")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(problem);

        _currentUserServiceMock.WithUser(adminUser);
        var handler = new DeleteSpraywallProblemCommandHandler(_dbContext, _currentUserServiceMock, _imageServiceMock.Object);
        await handler.HandleAsync(new DeleteSpraywallProblemCommand { Id = problem.Id });

        var exists = await _dbContext.SpraywallProblems.AnyAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(exists);

        _imageServiceMock.Verify(imageService => imageService.DeleteImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }
}
