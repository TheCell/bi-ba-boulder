using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetLineTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public GetLineTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task GetLine_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.User).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line1 = new LineBuilder().SetIdentifier("L-001").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetIdentifier("L-002").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([line1, line2]);

        var handler = new GetLineQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLineQuery { Id = line2.Id });

        var resultLine2 = result;
        LineAssertion.Assert(line2, resultLine2);
        Assert.False(resultLine2.Metadata.CanEdit);
        Assert.False(resultLine2.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetLine_AsAdmin_ReturnsMatchingLine()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.Admin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line1 = new LineBuilder().SetIdentifier("L-001").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetIdentifier("L-002").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([line1, line2]);

        var handler = new GetLineQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLineQuery { Id = line2.Id });

        var resultLine2 = result;
        LineAssertion.Assert(line2, resultLine2);
        Assert.False(resultLine2.Metadata.CanEdit);
        Assert.True(resultLine2.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetLine_AsCreator_ReturnsMatchingLine()
    {
        var user = new UserBuilder().SetRoles($"{AuthorizationRoles.Admin},{AuthorizationRoles.Editor}").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line1 = new LineBuilder().SetCreator(user).SetIdentifier("L-001").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetCreator(user).SetIdentifier("L-002").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([line1, line2]);

        var handler = new GetLineQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLineQuery { Id = line2.Id });

        var resultLine2 = result;
        LineAssertion.Assert(line2, resultLine2);
        Assert.True(resultLine2.Metadata.CanEdit);
        Assert.True(resultLine2.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetLine_NotFoundException()
    {
        var handler = new GetLineQueryHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(new GetLineQuery { Id = Guid.CreateVersion7() }));
    }
}
