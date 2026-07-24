using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class GetLinesByBlocIdTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;

    public GetLinesByBlocIdTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task GetLinesByBlocId_ReturnsMatchingLines()
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

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(2, result.Count);
        var resultLine1 = result.First(l => l.Id == line1.Id);
        LineAssertion.Assert(line1, resultLine1);
        Assert.False(resultLine1.Metadata.CanEdit);
        Assert.False(resultLine1.Metadata.CanDelete);
        var resultLine2 = result.First(l => l.Id == line2.Id);
        LineAssertion.Assert(line2, resultLine2);
        Assert.False(resultLine2.Metadata.CanEdit);
        Assert.False(resultLine2.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetLinesByBlocId_AsAdmin_ReturnsMatchingLines()
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

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(2, result.Count);
        var resultLine1 = result.First(l => l.Id == line1.Id);
        LineAssertion.Assert(line1, resultLine1);
        Assert.False(resultLine1.Metadata.CanEdit);
        Assert.True(resultLine1.Metadata.CanDelete);
        var resultLine2 = result.First(l => l.Id == line2.Id);
        LineAssertion.Assert(line2, resultLine2);
        Assert.False(resultLine2.Metadata.CanEdit);
        Assert.True(resultLine2.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetLinesByBlocId_AsCreator_ReturnsMatchingLines()
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

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(2, result.Count);
        var resultLine1 = result.First(l => l.Id == line1.Id);
        LineAssertion.Assert(line1, resultLine1);
        Assert.True(resultLine1.Metadata.CanEdit);
        Assert.True(resultLine1.Metadata.CanDelete);
        var resultLine2 = result.First(l => l.Id == line2.Id);
        LineAssertion.Assert(line2, resultLine2);
        Assert.True(resultLine2.Metadata.CanEdit);
        Assert.True(resultLine2.Metadata.CanDelete);
    }

    [Fact]
    public async Task GetLinesByBlocId_SortedByIdentifier()
    {
        var sector = new SectorBuilder().SetName("Sector").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var bloc = new BlocBuilder().SetName("Bloc").SetSectorId(sector.Id).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var line1 = new LineBuilder().SetIdentifier("1.").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line2 = new LineBuilder().SetIdentifier("2.").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line3 = new LineBuilder().SetIdentifier("3.").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var linea = new LineBuilder().SetIdentifier("A").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var lineb = new LineBuilder().SetIdentifier("B").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var linec = new LineBuilder().SetIdentifier("C").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var line1C = new LineBuilder().SetIdentifier("1C").SetData(new LineData { Positions = [[1.0, 1.1, 1.2], [2.0, 2.1, 2.2]] }).SetBlocId(bloc.Id).Build();
        var array = new List<Line> { line1, line2, line3, linea, lineb, linec, line1C };
        var correctOrder = new List<Line> { line1, line1C, line2, line3, linea, lineb, linec };
        await _dbContext.InsertEntitiesAndSaveChangesAsync(array.Shuffle());

        var handler = new GetLinesByBlocIdQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = bloc.Id });

        Assert.Equal(7, result.Count);
        var resultList = result.ToList();
        for (var i = 0; i < correctOrder.Count; i++)
        {
            LineAssertion.Assert(correctOrder[i], resultList[i]);
        }
    }

    [Fact]
    public async Task GetLinesByBlocId_NoLines_ReturnsEmpty()
    {
        var handler = new GetLinesByBlocIdQueryHandler(_dbContext, _currentUserServiceMock);
        var result = await handler.HandleAsync(new GetLinesByBlocIdQuery { BlocId = Guid.CreateVersion7() });

        Assert.Empty(result);
    }
}
