using System;
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
    public async Task GetLinesByBlocId_NoLines_ReturnsEmpty()
    {
        var handler = new GetLinesByBlocIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = Guid.CreateVersion7() });

        Assert.Empty(result);
    }
}
