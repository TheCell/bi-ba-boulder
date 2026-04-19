using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class GetUserByIdTest
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public GetUserByIdTest()
    {
        _dbContext = new DbContextMock().Build();
    }

    [Fact]
    public async Task GetUserById_Ok()
    {
        var user = new UserBuilder().SetEmail("admin@test.com").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var handler = new GetUserByIdQueryHandler(_dbContext);
        var result = await handler.HandleAsync(new GetUserByIdQuery { Id = user.Id });

        UserAssertion.Assert(user, result);
    }

    [Fact]
    public async Task GetUserById_NotFound_ThrowsNotFoundException()
    {
        var handler = new GetUserByIdQueryHandler(_dbContext);

        var guid = Guid.CreateVersion7();
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetUserByIdQuery { Id = guid }));
        Assert.Equal($"User not found. (Id: {guid})", ex.Message);
    }
}
