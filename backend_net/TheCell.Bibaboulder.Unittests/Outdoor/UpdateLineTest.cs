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
            Data = CreateLineData(false)
        };

        var handler = new UpdateLineCommandHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateLine_ArgumentException()
    {
        var line = await PrepareLine(false);

        var command = new UpdateLineCommand
        {
            Id = line.Id,
            Version = line.Version,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = new LineData
            {
                Positions =
                [
                    [1.0, 1.1, 1.2],
                    [2.0, 2.1, 2.2]
                ]
            }
        };

        var handler = new UpdateLineCommandHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("A line must have at least 3 positions.", ex.Message);
    }

    [Fact]
    public async Task UpdateLine_MissingLineData_ArgumentException()
    {
        var line = await PrepareLine(false);

        var command = new UpdateLineCommand
        {
            Id = line.Id,
            Version = line.Version,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = new LineData()
        };

        var handler = new UpdateLineCommandHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateLine_Ok()
    {
        var line = await PrepareLine(false);

        var command = new UpdateLineCommand
        {
            Id = line.Id,
            Version = line.Version,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = CreateLineData(false)
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
        LineDataAssertion.Assert(command.Data, updated.Data);
    }

    [Fact]
    public async Task UpdateLine_AddLineData_Ok()
    {
        var line = await PrepareLine(false);

        var command = new UpdateLineCommand
        {
            Id = line.Id,
            Version = line.Version,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = CreateLineData(true)
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
        LineDataAssertion.Assert(command.Data, updated.Data);
    }

    [Fact]
    public async Task UpdateLine_RemoveLineData_Ok()
    {
        var line = await PrepareLine(true);

        var command = new UpdateLineCommand
        {
            Id = line.Id,
            Version = line.Version,
            Identifier = "L-002",
            Name = "Updated line",
            Description = "Updated description",
            Data = CreateLineData(false)
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
        LineDataAssertion.Assert(command.Data, updated.Data);
    }

    private async Task<Line> PrepareLine(bool withSceneMarkings)
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line = new LineBuilder()
            .SetIdentifier("L-001")
            .SetName("Line")
            .SetDescription("Description")
            .SetData(CreateLineData(withSceneMarkings))
            .SetBlocId(bloc.Id)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(line);

        return line;
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
