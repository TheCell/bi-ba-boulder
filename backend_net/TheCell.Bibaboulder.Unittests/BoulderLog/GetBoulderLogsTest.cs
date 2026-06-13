using System.Threading.Tasks;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Model;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.BoulderLog;

public class GetBoulderLogsTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogsTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetBoulderLogs_ReturnsOnlyCurrentUserLogs_Ok()
    {
        var user1 = new UserBuilder().SetOidcSubject("user1").Build();
        var user2 = new UserBuilder().SetOidcSubject("user2").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user1);
        await _dbContext.InsertEntityAndSaveChangesAsync(user2);

        var log1 = new BoulderLogBuilder().SetUser(user1).SetIsSent(true).Build();
        var log2 = new BoulderLogBuilder().SetUser(user1).SetIsSent(false).Build();
        var log3 = new BoulderLogBuilder().SetUser(user2).SetIsSent(true).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(log1);
        await _dbContext.InsertEntityAndSaveChangesAsync(log2);
        await _dbContext.InsertEntityAndSaveChangesAsync(log3);

        var query = new GetBoulderLogsQuery { UserId = user1.Id };
        var handler = new GetBoulderLogsQueryHandler(_dbContext);
        var result = await handler.HandleAsync(query);

        Assert.Equal(2, result.Count);
    }
}
