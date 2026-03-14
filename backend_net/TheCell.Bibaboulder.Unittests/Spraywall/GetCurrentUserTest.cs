using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class GetCurrentUserTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    public GetCurrentUserTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task GetCurrentUser_Ok()
    {
        var user = new UserBuilder().Build();
        await _dbContext.InsertEntityAsync(user);

        _currentUserServiceMock.WithUser(user);
        var handler = new GetCurrentUserQueryHandler(_currentUserServiceMock);
        var result = await handler.HandleAsync(new GetCurrentUserQuery { });

        UserAssertion.Assert(user, result);
    }

    [Fact]
    public async Task GetCurrentUser_NotFoundException()
    {
        var handler = new GetCurrentUserQueryHandler(_currentUserServiceMock);

        var guid = Guid.CreateVersion7();
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetCurrentUserQuery { }));
        Assert.Equal("No current user configured in mock.", ex.Message);
    }
}
