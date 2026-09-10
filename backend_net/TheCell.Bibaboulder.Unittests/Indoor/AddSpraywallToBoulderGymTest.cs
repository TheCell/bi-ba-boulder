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

public class AddSpraywallToBoulderGymTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserService;

    public AddSpraywallToBoulderGymTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserService = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task AddSpraywallToBoulderGym_NotAuthorized_UnauthorizedAccessException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserService.WithUser(user);

        var command = new AddSpraywallToBoulderGymCommand { BoulderGymId = Guid.CreateVersion7(), SpraywallId = Guid.CreateVersion7() };
        var handler = new AddSpraywallToBoulderGymCommandHandler(_dbContext, _currentUserService);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task AddSpraywallToBoulderGym_GymNotFound_NotFoundException()
    {
        var admin = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(admin);
        _currentUserService.WithUser(admin);

        var command = new AddSpraywallToBoulderGymCommand { BoulderGymId = Guid.CreateVersion7(), SpraywallId = Guid.CreateVersion7() };
        var handler = new AddSpraywallToBoulderGymCommandHandler(_dbContext, _currentUserService);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"BoulderGym not found. (Id: {command.BoulderGymId})", ex.Message);
    }

    [Fact]
    public async Task AddSpraywallToBoulderGym_SpraywallNotFound_NotFoundException()
    {
        var admin = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(admin);
        _currentUserService.WithUser(admin);

        var boulderGym = new BoulderGymBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new AddSpraywallToBoulderGymCommand { BoulderGymId = boulderGym.Id, SpraywallId = Guid.CreateVersion7() };
        var handler = new AddSpraywallToBoulderGymCommandHandler(_dbContext, _currentUserService);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"Spraywall not found. (Id: {command.SpraywallId})", ex.Message);
    }

    [Fact]
    public async Task AddSpraywallToBoulderGym_Ok()
    {
        var admin = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(admin);
        _currentUserService.WithUser(admin);

        var boulderGym = new BoulderGymBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var spraywall = new SpraywallBuilder().SetBoulderGymId(null).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var command = new AddSpraywallToBoulderGymCommand { BoulderGymId = boulderGym.Id, SpraywallId = spraywall.Id };
        var handler = new AddSpraywallToBoulderGymCommandHandler(_dbContext, _currentUserService);

        await handler.HandleAsync(command);

        var updatedSpraywall = await _dbContext.Spraywalls
            .SingleAsync(s => s.Id == spraywall.Id, TestContext.Current.CancellationToken);

        Assert.Equal(boulderGym.Id, updatedSpraywall.BoulderGymId);
    }
}
