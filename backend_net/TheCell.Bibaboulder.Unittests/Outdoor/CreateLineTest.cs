using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
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
    public async Task CreateLineForBloc_NotFound_NotFoundException()
    {
        var command = new CreateLineCommand
        {
            BlocId = Guid.CreateVersion7(),
            Identifier = "L-001",
            Name = "Line",
            Description = "Description",
            Data = CreateLineData(false)
        };

        var handler = new CreateLineCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateLineForBloc_ArgumentException()
    {
        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            BlocId = bloc.Id,
            Identifier = "L-001",
            Name = "Line",
            Description = "Description",
            Data = new LineData { Positions = [] }
        };

        var handler = new CreateLineCommandHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("A line must have at least 3 positions.", ex.Message);
    }

    [Fact]
    public async Task CreateLineForBloc_Ok()
    {
        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            BlocId = bloc.Id,
            Identifier = "L-001",
            Name = "Line",
            Description = "Description",
            Data = CreateLineData(false)
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
        LineDataAssertion.Assert(command.Data, line.Data);
        Assert.Equal(1, line.Version);
    }

    [Fact]
    public async Task CreateLineForBloc_WithMarkings_Ok()
    {
        var bloc = await PrepareBloc();

        var command = new CreateLineCommand
        {
            BlocId = bloc.Id,
            Identifier = "L-001",
            Name = "Line",
            Description = "Description",
            Data = CreateLineData(true)
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
        LineDataAssertion.Assert(command.Data, line.Data);
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

    private static LineData CreateLineData(bool withSceneMarkings)
    {
        var lineData = new LineData
        {
            Positions =
            [
                [1.0, 1.1, 1.2],
                [2.0, 2.1, 2.2],
                [3.0, 3.1, 3.2]
            ]
        };
        if (withSceneMarkings)
        {
            lineData.SceneMarkings = [
                new SceneMarking{
                    Position = [1.0, 1.1, 1.2],
                    Quaternion = [0.12, 0.0, 0.0, 1.0],
                    Scale = [1.0, 1.0, 1.0],
                    Type = SceneMarkingType.Start,
                    Form = SceneMarkingForm.Sphere,
                },
                new SceneMarking{
                    Position = [2.0, 2.1, 2.2],
                    Quaternion = [0.44, 0.11, 0.22, 1.0],
                    Scale = [2.0, 2.0, 2.0],
                    Type = SceneMarkingType.Top,
                    Form = SceneMarkingForm.Box,
                },
                new SceneMarking{
                    Position = [3.0, 3.1, 3.2],
                    Quaternion = [0.22, 0.78, 0.53, 1.0],
                    Scale = [3.0, 3.0, 3.0],
                    Type = SceneMarkingType.OffLine,
                    Form = SceneMarkingForm.Sphere,
                },
                new SceneMarking{
                    Position = [4.0, 4.1, 4.2],
                    Quaternion = [0.12, 0.45, 0.45, 1.0],
                    Scale = [4.0, 4.0, 4.0],
                    Type = SceneMarkingType.OffLine,
                    Form = SceneMarkingForm.Box,
                }
                ];
        }

        return lineData;
    }
}
