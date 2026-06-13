using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Model;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.BoulderLog;

public class GetBoulderLogBySpraywallTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetBoulderLogBySpraywallTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetBoulderLogBySpraywall_NotFound_Ok()
    {
        var query = new GetBoulderLogBySpraywallQuery
        {
            SpraywallProblemId = Guid.CreateVersion7()
        };

        var handler = new GetBoulderLogBySpraywallQueryHandler(_dbContext);

        var result = await handler.HandleAsync(query);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoulderLogBySpraywall_Ok()
    {
        var user = new UserBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var spraywall = new SpraywallBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywall);

        var spraywallProblem = new SpraywallProblemBuilder(user, spraywall).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(spraywallProblem);

        var boulderLog = new BoulderLogBuilder()
            .SetUser(user)
            .SetIsSent(true)
            .SetIsProject(false)
            .SetSpraywallProblem(spraywallProblem)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderLog);

        var query = new GetBoulderLogBySpraywallQuery
        {
            SpraywallProblemId = spraywallProblem.Id
        };

        var handler = new GetBoulderLogBySpraywallQueryHandler(_dbContext);
        var result = await handler.HandleAsync(query);

        Assert.NotNull(result);
        BoulderLogAssertion.Assert(boulderLog, result);
    }
}
