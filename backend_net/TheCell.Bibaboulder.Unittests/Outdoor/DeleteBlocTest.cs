using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class DeleteBlocTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public DeleteBlocTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task DeleteBloc_NotFound_NotFoundException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var command = new DeleteBlocCommand { Id = Guid.CreateVersion7(), Version = 1 };
        var handler = new DeleteBlocCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteBloc_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = user.Id;
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new DeleteBlocCommand { Id = bloc.Id, Version = bloc.Version };
        var handler = new DeleteBlocCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Blocs.AnyAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }
}
