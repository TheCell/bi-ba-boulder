using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetSectorTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserService;

    public GetSectorTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserService = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task GetSector_NotFoundException()
    {
        var query = new GetSectorQuery
        {
            Id = Guid.CreateVersion7()
        };

        var handler = new GetSectorQueryHandler(_dbContext, _currentUserService);

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
            .SetImportantInfo("Test Important Info")
            .SetIsPublic(true)
            .SetCoordinates("46.9914628, 7.5589870")
            .SetPreviewImageUri("https://example.com/preview.jpg")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/image.jpg").SetResourceType(ResourceType.Image).Build()])
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var query = new GetSectorQuery
        {
            Id = sector.Id
        };

        var handler = new GetSectorQueryHandler(_dbContext, _currentUserService);
        var result = await handler.HandleAsync(query);

        SectorAssertion.Assert(sector, result);
        Assert.Equal(1, sector.Version);
    }

    [Fact]
    public async Task GetSector_HidesPrivateOutdoorAreas()
    {
        var publicArea = new OutdoorAreaBuilder().SetIsPublic(true).Build();
        var privateArea = new OutdoorAreaBuilder().Build();
        var sector = new SectorBuilder().Build();
        sector.OutdoorAreas = [publicArea, privateArea];
        await _dbContext.InsertEntitiesAndSaveChangesAsync([publicArea, privateArea, sector]);

        var handler = new GetSectorQueryHandler(_dbContext, _currentUserService);

        var result = await handler.HandleAsync(new GetSectorQuery { Id = sector.Id });

        Assert.Equal(publicArea.Id, Assert.Single(result.OutdoorAreas).Id);
    }
}
