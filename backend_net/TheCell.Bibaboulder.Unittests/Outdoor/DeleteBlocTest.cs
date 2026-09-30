using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class DeleteBlocTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteBlocTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task DeleteBloc_NotFound_NotFoundException()
    {
        var command = new DeleteBlocCommand { Id = Guid.CreateVersion7(), Version = 1 };
        var handler = new DeleteBlocCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteBloc_Ok()
    {
        var sector = new SectorBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new DeleteBlocCommand { Id = bloc.Id, Version = bloc.Version };
        var handler = new DeleteBlocCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Blocs.AnyAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }
}
