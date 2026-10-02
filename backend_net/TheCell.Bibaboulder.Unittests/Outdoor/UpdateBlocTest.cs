using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class UpdateBlocTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Faker _bogus;

    public UpdateBlocTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task UpdateBloc_NotFound_NotFoundException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var command = new UpdateBlocCommand
        {
            Id = Guid.CreateVersion7(),
            SectorId = Guid.CreateVersion7(),
            Name = _bogus.Lorem.Slug(),
            Version = 1
        };
        var handler = new UpdateBlocCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"Bloc not found. (Id: {command.Id})", ex.Message);
    }

    [Fact]
    public async Task UpdateBloc_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().Build();
        var otherSector = new SectorBuilder().Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([sector, otherSector]);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = user.Id;
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand
        {
            Id = bloc.Id,
            Version = bloc.Version,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var handler = new UpdateBlocCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var updated = await _dbContext.Blocs
            .AsNoTracking()
            .SingleAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        BlocAssertion.Assert(command, updated);
    }

    [Fact]
    public async Task UpdateBloc_ChangeSector_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().Build();
        var otherSector = new SectorBuilder().Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([sector, otherSector]);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = user.Id;
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand
        {
            Id = bloc.Id,
            Version = bloc.Version,
            SectorId = otherSector.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var handler = new UpdateBlocCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var updated = await _dbContext.Blocs
            .AsNoTracking()
            .SingleAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        BlocAssertion.Assert(command, updated);
    }

    [Fact]
    public async Task UpdateBloc_RemoveSector_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().Build();
        var otherSector = new SectorBuilder().Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([sector]);
        var bloc = new BlocBuilder().SetSectorId(sector.Id).Build();
        bloc.CreatedUserId = user.Id;
        await _dbContext.InsertEntityAndSaveChangesAsync(bloc);

        var command = new UpdateBlocCommand
        {
            Id = bloc.Id,
            Version = bloc.Version,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var handler = new UpdateBlocCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var updated = await _dbContext.Blocs
            .AsNoTracking()
            .SingleAsync(item => item.Id == bloc.Id, TestContext.Current.CancellationToken);
        Assert.Null(updated.SectorId);
        BlocAssertion.Assert(command, updated);
    }
}
