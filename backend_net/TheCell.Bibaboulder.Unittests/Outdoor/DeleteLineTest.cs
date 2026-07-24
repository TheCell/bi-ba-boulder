using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class DeleteLineTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public DeleteLineTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task DeleteLine_NotFound_NotFoundException()
    {
        var command = new DeleteLineCommand
        {
            Id = Guid.CreateVersion7(),
            Version = 1
        };

        var handler = new DeleteLineCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task DeleteLine_Ok()
    {
        var admin = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(admin);
        _currentUserServiceMock.WithUser(admin);

        var line = await PrepareLine();

        var command = new DeleteLineCommand
        {
            Id = line.Id,
            Version = line.Version
        };

        var handler = new DeleteLineCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Lines.AnyAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteLine_AsAdmin_Ok()
    {
        var admin = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(admin);
        _currentUserServiceMock.WithUser(admin);

        var creator = new UserBuilder().SetRoles(AuthorizationRoles.Editor).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creator);

        var line = await PrepareLine(creator);

        var command = new DeleteLineCommand
        {
            Id = line.Id,
            Version = line.Version
        };

        var handler = new DeleteLineCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Lines.AnyAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteLine_AsCreator_Ok()
    {
        var creator = new UserBuilder().SetRoles($"{AuthorizationRoles.User},{AuthorizationRoles.Editor}").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creator);
        _currentUserServiceMock.WithUser(creator);

        var line = await PrepareLine(creator);

        var command = new DeleteLineCommand
        {
            Id = line.Id,
            Version = line.Version
        };

        var handler = new DeleteLineCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var exists = await _dbContext.Lines.AnyAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteLine_AsOtherCreator_ThrowsException()
    {
        var creator = new UserBuilder().SetRoles($"{AuthorizationRoles.User},{AuthorizationRoles.Editor}").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(creator);

        var otherUser = new UserBuilder().SetRoles(AuthorizationRoles.Editor).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(otherUser);
        _currentUserServiceMock.WithUser(otherUser);

        var line = await PrepareLine(creator);

        var command = new DeleteLineCommand
        {
            Id = line.Id,
            Version = line.Version
        };

        var handler = new DeleteLineCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<AccessDeniedException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("Only the creator or an admin can delete this line", ex.Message);

        var exists = await _dbContext.Lines.AnyAsync(l => l.Id == line.Id, TestContext.Current.CancellationToken);
        Assert.True(exists);
    }

    private async Task<Line> PrepareLine(User? creator = null)
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var builder = new LineBuilder()
            .SetIdentifier("L-001")
            .SetName("Line")
            .SetDescription("Description")
            .SetData(CreateLineData())
            .SetBlocId(bloc.Id);

        if (creator is not null)
        {
            builder.SetCreator(creator);
        }

        var line = builder.Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(line);

        return line;
    }

    private static LineData CreateLineData()
    {
        return new LineData
        {
            Positions =
            [
                [1.0, 1.1, 1.2],
                [2.0, 2.1, 2.2]
            ]
        };
    }
}
