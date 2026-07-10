using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class CreateLineTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public CreateLineTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task CreateLine_NotFound_NotFoundException()
    {
        var command = new CreateLineCommand
        {
            BlocId = Guid.CreateVersion7(),
            Identifier = "L-001",
            Name = "Line",
            Description = "Description",
            Data = CreateLineData()
        };

        var handler = new CreateLineCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateLine_Ok()
    {
        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            BlocId = bloc.Id,
            Identifier = "L-001",
            Name = "Line",
            Description = "Description",
            Data = CreateLineData()
        };

        var handler = new CreateLineCommandHandler(_dbContext);
        await handler.HandleAsync(command);

        var line = await _dbContext.Lines
            .AsNoTracking()
            .SingleAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(command.Id, line.Id);
        Assert.Equal(command.BlocId, line.BlocId);
        Assert.Equal(command.Identifier, line.Identifier);
        Assert.Equal(command.Name, line.Name);
        Assert.Equal(command.Description, line.Description);
        LineAssertion.Assert(command.Data, line.Data);
        Assert.Equal(1, line.Version);
    }

    private async Task<Bloc> PrepareBloc()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);
        return bloc;
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