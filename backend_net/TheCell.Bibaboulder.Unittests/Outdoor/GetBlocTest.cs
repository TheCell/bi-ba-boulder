using System;
using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetBlocTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBlocTest()
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
        var sector = new SectorBuilder()
            .SetName("Sector")
            .SetCoordinates("46.9929293, 7.5582829")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder()
            .SetName("TestBloc")
            .SetCoordinates("46.9929293, 7.5582829")
            .SetSectorId(sector.Id)
            .SetAdditionalParts([
                new BlocBuilder()
                    .SetName("part 2)")
                    .SetDescription("part 2 description")
                    .SetCoordinates("46.9929293, 7.5582829")
                    .SetBlocLowRes("part_2_low_res/newpart.glb")
                    .SetBlocMedRes("part_2_med_res/newpart.glb")
                    .SetBlocHighRes("part_2_high_res/newpart.glb")
                    .Build(),
                new BlocBuilder()
                    .SetName("part 3)")
                    .SetDescription("part 3 description")
                    .SetCoordinates("46.9929293, 7.5582829")
                    .SetBlocLowRes("part_3_low_res/newpart.glb")
                    .SetBlocMedRes("part_3_med_res/newpart.glb")
                    .SetBlocHighRes("part_3_high_res/newpart.glb")
                    .Build(),
                new BlocBuilder()
                    .SetName("part 4)")
                    .SetDescription("part 4 description")
                    .SetCoordinates("46.9929293, 7.5582829")
                    .SetBlocLowRes("part_4_low_res/newpart.glb")
                    .SetBlocMedRes("part_4_med_res/newpart.glb")
                    .SetBlocHighRes("part_4_high_res/newpart.glb")
                    .Build(),
            ])
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var handler = new GetBlocQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBlocQuery { Id = bloc.Id });

        BlocAssertion.Assert(bloc, result);
    }

    [Fact]
    public async Task GetBlocsBySector_NoBlocs_Ok()
    {
        var sector = new SectorBuilder()
            .SetName("Sector")
            .SetCoordinates("46.9929293, 7.5582829")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var handler = new GetBlocsBySectorIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBlocsBySectorIdQuery { SectorId = sector.Id });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBlocsBySector_Ok()
    {
        var sector = new SectorBuilder()
            .SetName("Sector")
            .SetCoordinates("46.9929293, 7.5582829")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc1 = new BlocBuilder()
            .SetName("TestBloc 1")
            .SetCoordinates("46.9929293, 7.5582829")
            .SetSectorId(sector.Id)
            .Build();
        var bloc2 = new BlocBuilder()
            .SetName("TestBloc 2")
            .SetCoordinates("46.9929293, 7.5582829")
            .SetSectorId(sector.Id)
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([bloc1, bloc2]);

        var handler = new GetBlocsBySectorIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetBlocsBySectorIdQuery { SectorId = sector.Id });

        Assert.Equal(2, result.Count);
        BlocAssertion.Assert(bloc1, result.Single(b => b.Id == bloc1.Id));
        BlocAssertion.Assert(bloc2, result.Single(b => b.Id == bloc2.Id));
    }
}
