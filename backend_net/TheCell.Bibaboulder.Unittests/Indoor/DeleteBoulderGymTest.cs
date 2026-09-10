using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Indoor;

public class DeleteBoulderGymTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserService;

    public DeleteBoulderGymTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserService = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task DeleteBoulderGym_NotFound_NotFoundException()
    {
        var command = new DeleteBoulderGymCommand { Id = Guid.CreateVersion7(), Version = 1 };
        var handler = new DeleteBoulderGymCommandHandler(_dbContext, _currentUserService);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteBoulderGym_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var boulderGym = new BoulderGymBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new DeleteBoulderGymCommand { Id = boulderGym.Id, Version = boulderGym.Version };
        var handler = new DeleteBoulderGymCommandHandler(_dbContext, _currentUserService);
        await handler.HandleAsync(command);

        var exists = await _dbContext.BoulderGyms.AnyAsync(gym => gym.Id == boulderGym.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    // todo test delete when boulder gym is linked to spraywall problem.
}
