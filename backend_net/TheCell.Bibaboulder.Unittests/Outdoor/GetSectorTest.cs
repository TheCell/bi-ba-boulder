using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetSectorTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSectorTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetSector_NotFoundException()
    {
        var query = new GetSectorQuery
        {
            Id = Guid.CreateVersion7()
        };

        var handler = new GetSectorQueryHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(query));
        Assert.Equal($"Sector not found. (Id: {query.Id})", ex.Message);
    }

    [Fact]
    public async Task GetSector_Ok()
    {
        var sector = new SectorBuilder()
            .SetName("Test Sector")
            .SetDescription("Test Description")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var query = new GetSectorQuery
        {
            Id = sector.Id
        };

        var handler = new GetSectorQueryHandler(_dbContext);
        var result = await handler.HandleAsync(query);

        SectorAssertion.Assert(sector, result);
        Assert.Equal(1, sector.Version);
    }
}
