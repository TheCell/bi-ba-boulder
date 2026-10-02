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

public class CreateBlocTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Faker _bogus;

    public CreateBlocTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateBloc_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var command = new CreateBlocCommand
        {
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var handler = new CreateBlocCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var bloc = await _dbContext.Blocs
            .AsNoTracking()
            .SingleAsync(item => item.Id == command.Id, TestContext.Current.CancellationToken);
        BlocAssertion.Assert(command, bloc);
        Assert.Equal(1, bloc.Version);
    }

    [Fact]
    public async Task CreateBloc_WithSector_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new CreateBlocCommand
        {
            SectorId = sector.Id,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            Coordinates = "46.9914628, 7.5589870",
            BlocLowRes = _bogus.Internet.UrlWithPath(),
            BlocMedRes = _bogus.Internet.UrlWithPath(),
            BlocHighRes = _bogus.Internet.UrlWithPath(),
            PreviewImageUri = _bogus.Internet.UrlWithPath()
        };

        var handler = new CreateBlocCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var bloc = await _dbContext.Blocs
            .AsNoTracking()
            .SingleAsync(item => item.Id == command.Id, TestContext.Current.CancellationToken);
        BlocAssertion.Assert(command, bloc);
        Assert.Equal(1, bloc.Version);
    }
}
