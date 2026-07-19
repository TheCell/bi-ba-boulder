using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetLinesByBlocIdTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetLinesByBlocIdTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetLinesByBlocId_ReturnsMatchingLines()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line1 = new LineBuilder().SetIdentifier("L-001").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetIdentifier("L-002").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([line1, line2]);

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(2, result.Count);
        LineAssertion.Assert(line1, result.First(l => l.Id == line1.Id));
        LineAssertion.Assert(line2, result.First(l => l.Id == line2.Id));
    }

    [Fact]
    public async Task GetLinesByBlocId_SortedByIdentifier()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line1 = new LineBuilder().SetIdentifier("1.").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetIdentifier("2.").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line3 = new LineBuilder().SetIdentifier("3.").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var linea = new LineBuilder().SetIdentifier("A").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var lineb = new LineBuilder().SetIdentifier("B").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var linec = new LineBuilder().SetIdentifier("C").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line1C = new LineBuilder().SetIdentifier("1C").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var array = new List<Line> { line1, line2, line3, linea, lineb, linec, line1C };
        var correctOrder = new List<Line> { line1, line1C, line2, line3, linea, lineb, linec };
        await _dbContext.InsertEntitiesAndSaveChangesAsync(array.Shuffle());

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(7, result.Count);
        var resultList = result.ToList();
        for (var i = 0; i < correctOrder.Count; i++)
        {
            LineAssertion.Assert(correctOrder[i], resultList[i]);
        }
    }

    [Fact]
    public async Task GetLinesByBlocId_NoLines_ReturnsEmpty()
    {
        var handler = new GetLinesByBlocIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = Guid.CreateVersion7() });

        Assert.Empty(result);
    }
}
