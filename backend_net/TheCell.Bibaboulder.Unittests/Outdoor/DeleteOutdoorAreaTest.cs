using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class DeleteOutdoorAreaTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteOutdoorAreaTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task DeleteOutdoorArea_NotFound_NotFoundException()
    {
        var command = new DeleteOutdoorAreaCommand { Id = Guid.CreateVersion7(), Version = 1 };
        var handler = new DeleteOutdoorAreaCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteOutdoorArea_Ok()
    {
        var outdoorArea = new OutdoorAreaBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new DeleteOutdoorAreaCommand { Id = outdoorArea.Id, Version = outdoorArea.Version };
        var handler = new DeleteOutdoorAreaCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var exists = await _dbContext.OutdoorAreas.AnyAsync(area => area.Id == outdoorArea.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    // todo test delete when spraywall is linked to it.
}
