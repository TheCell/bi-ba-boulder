using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Media.Handler;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Media;

public class GetUriAliasTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetUriAliasTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetUriAlias_NotFound_Ok()
    {
        var handler = new GetUriAliasQueryHandler(_dbContext);

        var result = await handler.HandleAsync(new GetUriAliasQuery
        {
            Alias = "test-alias",
            Type = UriType.OutdoorArea
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUriAlias_WrongType_Ok()
    {
        var uriAlias = new UriAliasBuilder("test-alias")
            .SetType(UriType.BoulderGym)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var handler = new GetUriAliasQueryHandler(_dbContext);

        var result = await handler.HandleAsync(new GetUriAliasQuery
        {
            Alias = uriAlias.Alias,
            Type = UriType.OutdoorArea
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetUriAlias_BoulderGym_Ok()
    {
        var boulderGym = new BoulderGymBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);
        var uriAlias = new UriAliasBuilder("test-boulder-gym")
            .SetType(UriType.BoulderGym)
            .SetBoulderGym(boulderGym)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var handler = new GetUriAliasQueryHandler(_dbContext);

        var result = await handler.HandleAsync(new GetUriAliasQuery
        {
            Alias = uriAlias.Alias,
            Type = uriAlias.Type
        });

        Assert.NotNull(result);
        Assert.Equal(boulderGym.Id, result.Id);
    }

    [Fact]
    public async Task GetUriAlias_OutdoorArea_Ok()
    {
        var outdoorArea = new OutdoorAreaBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);
        var uriAlias = new UriAliasBuilder("test-outdoor-area")
            .SetType(UriType.OutdoorArea)
            .SetOutdoorArea(outdoorArea)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var handler = new GetUriAliasQueryHandler(_dbContext);

        var result = await handler.HandleAsync(new GetUriAliasQuery
        {
            Alias = uriAlias.Alias,
            Type = uriAlias.Type
        });

        Assert.NotNull(result);
        Assert.Equal(outdoorArea.Id, result.Id);
    }

    [Fact]
    public async Task GetUriAlias_BoulderGymWithoutId_InvalidOperationException()
    {
        var uriAlias = new UriAliasBuilder("test-boulder-gym")
            .SetType(UriType.BoulderGym)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var handler = new GetUriAliasQueryHandler(_dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new GetUriAliasQuery
            {
                Alias = uriAlias.Alias,
                Type = uriAlias.Type
            }));

        Assert.Equal($"UriAlias with Id {uriAlias.Id} has type {UriType.BoulderGym} but no BoulderGymId.", exception.Message);
    }

    [Fact]
    public async Task GetUriAlias_OutdoorAreaWithoutId_InvalidOperationException()
    {
        var uriAlias = new UriAliasBuilder("test-outdoor-area")
            .SetType(UriType.OutdoorArea)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var handler = new GetUriAliasQueryHandler(_dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new GetUriAliasQuery
            {
                Alias = uriAlias.Alias,
                Type = uriAlias.Type
            }));

        Assert.Equal($"UriAlias with Id {uriAlias.Id} has type {UriType.OutdoorArea} but no OutdoorAreaId.", exception.Message);
    }

    [Fact]
    public async Task GetUriAlias_UnknownType_InvalidOperationException()
    {
        var unknownType = (UriType)999;
        var uriAlias = new UriAliasBuilder("test-alias")
            .SetType(unknownType)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);

        var handler = new GetUriAliasQueryHandler(_dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new GetUriAliasQuery
            {
                Alias = uriAlias.Alias,
                Type = uriAlias.Type
            }));

        Assert.Equal($"UriAlias with Id {uriAlias.Id} has an unknown type {unknownType}.", exception.Message);
    }
}