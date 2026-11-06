using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Indoor;

public class GetSpraywallTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetSpraywallTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetSpraywall_EmptyResult_NotFoundException()
    {
        var handler = new GetSpraywallQueryHandler(_dbContext);
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await handler.HandleAsync(new GetSpraywallQuery { Id = Guid.NewGuid() });
        });
    }

    [Fact]
    public async Task GetSpraywall_Ok()
    {
        var spraywall1 = new SpraywallBuilder()
            .SetName("Wall A")
            .SetPreviewImageUri("https://example.com/wall-a.jpg")
            .SetSpraywallModel("https://example.com/wall-a.glb")
            .SetUvMap("https://example.com/wall-a-uv.png")
            .Build();
        var spraywall2 = new SpraywallBuilder()
            .SetName("Wall B")
            .SetPreviewImageUri("https://example.com/wall-b.jpg")
            .SetSpraywallModel("https://example.com/wall-b.glb")
            .SetUvMap("https://example.com/wall-b-uv.png")
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([spraywall1, spraywall2]);

        var handler = new GetSpraywallQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetSpraywallQuery { Id = spraywall1.Id });

        SpraywallAssertion.Assert(spraywall1, result);
    }
}
