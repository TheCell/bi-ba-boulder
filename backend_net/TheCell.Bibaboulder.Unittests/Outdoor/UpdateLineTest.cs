using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class UpdateLineTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public UpdateLineTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task UpdateLine_NotFound_NotFoundException()
    {
        var command = new UpdateLineCommand
        {
            Id = Guid.CreateVersion7(),
            Version = 1,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = CreateLineData()
        };

        var handler = new UpdateLineCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateLine_Ok()
    {
        var line = await PrepareLine();

        var command = new UpdateLineCommand
        {
            Id = line.Id,
            Version = line.Version,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = CreateLineData()
        };

        var handler = new UpdateLineCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var updated = await _dbContext.Lines
            .AsNoTracking()
            .SingleAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);

        Assert.Equal(command.Identifier, updated.Identifier);
        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.Description, updated.Description);
        Assert.Equal(command.Version + 1, updated.Version);
        Assert.Equal(command.Data.Positions.Count, updated.Data.Positions.Count);
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