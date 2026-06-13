using System.Threading.Tasks;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.BoulderLog;

public class CreateBoulderLogTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public CreateBoulderLogTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task CreateBoulderLog_Anonymous_AccessDeniedException()
    {
        var command = new CreateBoulderLogCommand
        {
            IsSent = true,
            IsProject = false
        };

        var handler = new CreateBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("No current user configured in mock.", ex.Message);
    }

    [Fact]
    public async Task CreateBoulderLog_Ok()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.User)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateBoulderLogCommand
        {
            IsSent = true,
            IsProject = false,
            Rating = Rating.Three,
            FontGrade = FontGrade.ThreePlus
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var queryHandler = new GetBoulderLogQueryHandler(_dbContext);
        var result = await queryHandler.HandleAsync(new GetBoulderLogQuery { Id = command.Id });

        BoulderLogAssertion.Assert(command, result!);
        Assert.Equal(user.Id, result!.UserId);
    }
}
