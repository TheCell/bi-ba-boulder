using System;
using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class BlocsControllerTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public BlocsControllerTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetBloc_NotFoundException()
    {
        var handler = new GetBlocQueryHandler(_dbContext);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetBlocQuery { Id = Guid.CreateVersion7() }));
    }

    [Fact]
    public async Task GetBloc_Ok()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAsync(sector);

        var bloc = new BlocBuilder().SetName("TestBloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAsync(bloc);

        var handler = new GetBlocQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBlocQuery { Id = bloc.Id });

        BlocAssertion.Assert(bloc, result);
    }

    [Fact]
    public async Task GetBlocsBySector_NoBlocs_Ok()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAsync(sector);

        var handler = new GetBlocsBySectorIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBlocsBySectorIdQuery { SectorId = sector.Id });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBlocsBySector_Ok()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAsync(sector);

        var bloc1 = new BlocBuilder().SetName("TestBloc 1").SetSectorId(sector.Id).Build();
        var bloc2 = new BlocBuilder().SetName("TestBloc 2").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntitiesAsync([bloc1, bloc2]);

        var handler = new GetBlocsBySectorIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBlocsBySectorIdQuery { SectorId = sector.Id });

        Assert.Equal(2, result.Count);
        BlocAssertion.Assert(bloc1, result.Single(b => b.Id == bloc1.Id));
        BlocAssertion.Assert(bloc2, result.Single(b => b.Id == bloc2.Id));
    }
}
