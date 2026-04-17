using System;
using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
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
        await _dbContext.InsertEntityAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAsync(bloc);

        var line1 = new LineBuilder().SetIdentifier("L-001").SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetIdentifier("L-002").SetBlocId(bloc.Id).Build();
        await _dbContext.InsertEntitiesAsync([line1, line2]);

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(2, result.Count);
        LineAssertion.Assert(result.First(l => l.Id == line1.Id), line1);
        LineAssertion.Assert(result.First(l => l.Id == line2.Id), line2);
    }

    [Fact]
    public async Task GetLinesByBlocId_NoLines()
    {
        var handler = new GetLinesByBlocIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = Guid.CreateVersion7() });

        Assert.Empty(result);
    }
}
