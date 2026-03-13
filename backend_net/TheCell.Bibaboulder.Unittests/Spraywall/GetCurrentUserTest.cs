using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class GetCurrentUserTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetCurrentUserTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetCurrentUser_Ok()
    {
        var user = new UserBuilder().Build();
        await _dbContext.InsertEntityAsync(user);

        var handler = new GetCurrentUserQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetCurrentUserQuery { CurrentUserId = user.Id });

        UserAssertion.Assert(user, result);
    }

    [Fact]
    public async Task GetCurrentUser_NotFoundException()
    {
        var handler = new GetCurrentUserQueryHandler(_dbContext);

        var guid = Guid.CreateVersion7();
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetCurrentUserQuery { CurrentUserId = guid }));
        Assert.Equal(ex.Message, $"User not found. (Id: {guid})");
    }
}
