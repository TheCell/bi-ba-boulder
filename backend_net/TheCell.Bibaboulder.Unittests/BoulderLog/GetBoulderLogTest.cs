using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.BoulderLog;

public class GetBoulderLogTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetBoulderLog_NotFound_NotFoundException()
    {
        var query = new GetBoulderLogQuery
        {
            Id = Guid.CreateVersion7()
        };

        var handler = new GetBoulderLogQueryHandler(_dbContext);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(query));
        Assert.Contains("BoulderLog not found.", ex.Message);
    }

    [Fact]
    public async Task GetBoulderLog_Ok()
    {
        var user = new UserBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var boulderLog = new BoulderLogBuilder()
            .SetUserId(user.Id)
            .SetIsSent(true)
            .SetIsProject(false)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderLog);

        var query = new GetBoulderLogQuery
        {
            Id = boulderLog.Id
        };

        var handler = new GetBoulderLogQueryHandler(_dbContext);
        var result = await handler.HandleAsync(query);

        BoulderLogAssertion.Assert(boulderLog, result);
    }
}
