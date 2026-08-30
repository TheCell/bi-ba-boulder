using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class DeleteSectorTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteSectorTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task DeleteSector_NotFound_NotFoundException()
    {
        var command = new DeleteSectorCommand { Id = Guid.CreateVersion7(), Version = 1 };
        var handler = new DeleteSectorCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteSector_Ok()
    {
        var sector = new SectorBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new DeleteSectorCommand { Id = sector.Id, Version = sector.Version };
        var handler = new DeleteSectorCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Sectors.AnyAsync(item => item.Id == sector.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    // todo test delete when outdoor area is linked to it.
}
