using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.BoulderLog;

public class UpdateBoulderLogTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public UpdateBoulderLogTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task UpdateBoulderLog_NotFound_NotFoundException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var command = new UpdateBoulderLogCommand
        {
            Id = Guid.CreateVersion7(),
            Version = 1,
            IsSent = true,
            IsProject = false
        };

        var handler = new UpdateBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateBoulderLog_WrongUser_UnauthorizedException()
    {
        var owner = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        var otherUser = new UserBuilder().SetOidcSubject("other").SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(owner);
        await _dbContext.InsertEntityAndSaveChangesAsync(otherUser);

        var log = new BoulderLogBuilder().SetUser(owner).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(log);

        _currentUserServiceMock.WithUser(otherUser);

        var command = new UpdateBoulderLogCommand
        {
            Id = log.Id,
            Version = 1,
            IsSent = true,
            IsProject = true
        };

        var handler = new UpdateBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateBoulderLog_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var log = new BoulderLogBuilder().SetUser(user).SetIsSent(false).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(log);

        _currentUserServiceMock.WithUser(user);

        var command = new UpdateBoulderLogCommand
        {
            Id = log.Id,
            Version = 1,
            IsSent = true,
            IsProject = true,
            Rating = Rating.Five,
            FontGrade = FontGrade.FourMinus
        };

        var handler = new UpdateBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var queryHandler = new GetBoulderLogQueryHandler(_dbContext);
        var result = await queryHandler.HandleAsync(new GetBoulderLogQuery { Id = log.Id });

        Assert.True(result!.IsSent);
        Assert.True(result.IsProject);
        Assert.Equal(Rating.Five, result.Rating);
        Assert.Equal(FontGrade.FourMinus, result.FontGrade);
    }
}
