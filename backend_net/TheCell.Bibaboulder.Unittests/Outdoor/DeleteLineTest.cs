using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class DeleteLineTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteLineTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task DeleteLine_NotFound_NotFoundException()
    {
        var command = new DeleteLineCommand
        {
            Id = Guid.CreateVersion7(),
            Version = 1
        };

        var handler = new DeleteLineCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteLine_Ok()
    {
        var line = await PrepareLine();

        var command = new DeleteLineCommand
        {
            Id = line.Id,
            Version = line.Version
        };

        var handler = new DeleteLineCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Lines.AnyAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    private async Task<Line> PrepareLine()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line = new LineBuilder()
            .SetIdentifier("L-001")
            .SetName("Line")
            .SetDescription("Description")
            .SetData(CreateLineData())
            .SetBlocId(bloc.Id)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(line);

        return line;
    }

    private static LineData CreateLineData()
    {
        return new LineData
        {
            Positions =
            [
                [1.0, 1.1, 1.2],
                [2.0, 2.1, 2.2]
            ]
        };
    }
}