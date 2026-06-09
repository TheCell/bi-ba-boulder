using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.BoulderLog;

public class DeleteBoulderLogTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public DeleteBoulderLogTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task DeleteBoulderLog_NotFound_NotFoundException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var command = new DeleteBoulderLogCommand
        {
            Id = Guid.CreateVersion7(),
            Version = 1
        };

        var handler = new DeleteBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteBoulderLog_WrongUser_UnauthorizedException()
    {
        var owner = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        var otherUser = new UserBuilder().SetOidcSubject("other").SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(owner);
        await _dbContext.InsertEntityAndSaveChangesAsync(otherUser);

        var log = new BoulderLogBuilder().SetUserId(owner.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(log);

        _currentUserServiceMock.WithUser(otherUser);

        var command = new DeleteBoulderLogCommand
        {
            Id = log.Id,
            Version = 1
        };

        var handler = new DeleteBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteBoulderLog_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var log = new BoulderLogBuilder().SetUserId(user.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(log);

        _currentUserServiceMock.WithUser(user);

        var command = new DeleteBoulderLogCommand
        {
            Id = log.Id,
            Version = 1
        };

        var handler = new DeleteBoulderLogCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var exists = await _dbContext.BoulderLogs.AnyAsync(b => b.Id == log.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }
}
