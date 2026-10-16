using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class UpdateSectorTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Faker _bogus;

    public UpdateSectorTest()
    {
        _dbContext = new DbContextMock().Build();
        _bogus = new Faker("de_CH");
        _currentUserServiceMock = new CurrentUserServiceMock();
    }

    [Fact]
    public async Task UpdateSector_NotFound_NotFoundException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.Editor).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var command = new UpdateSectorCommand { Id = Guid.CreateVersion7(), Name = _bogus.Lorem.Slug(), Version = 1 };
        var handler = new UpdateSectorCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"Sector not found. (Id: {command.Id})", ex.Message);
    }

    [Fact]
    public async Task UpdateSector_UnknownOutdoorArea_ArgumentException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var sector = new SectorBuilder().SetCreatedUser(user).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new UpdateSectorCommand
        {
            Id = sector.Id,
            Version = sector.Version,
            Name = sector.Name,
            OutdoorAreaIds = [Guid.CreateVersion7()]
        };

        var handler = new UpdateSectorCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
        Assert.Equal("One or more outdoor areas do not exist.", ex.Message);
    }

    [Fact]
    public async Task UpdateSector_ReplacesOutdoorAreasAndImages_Unauthorized()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var outdoorArea = new OutdoorAreaBuilder().SetName("Lindental").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var sector = new SectorBuilder().SetName("Original").SetIsPublic(false).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new UpdateSectorCommand
        {
            Id = sector.Id,
            Version = sector.Version,
            Name = _bogus.Lorem.Slug(),
            Coordinates = "46.9914628, 7.5589870",
            IsPublic = true,
            ImageUris = [_bogus.Internet.Url()
                .Replace("https://", "")
                .Replace("http://", "")],
            OutdoorAreaIds = [outdoorArea.Id]
        };

        var handler = new UpdateSectorCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateSector_ReplacesOutdoorAreasAndImages_Ok()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);
        _currentUserServiceMock.WithUser(user);

        var outdoorArea = new OutdoorAreaBuilder().SetName("Lindental").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var sector = new SectorBuilder().SetName("Original").SetIsPublic(false).SetCreatedUser(user).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new UpdateSectorCommand
        {
            Id = sector.Id,
            Version = sector.Version,
            Name = _bogus.Lorem.Slug(),
            Coordinates = "46.9914628, 7.5589870",
            IsPublic = true,
            ImageUris = [_bogus.Internet.Url()
                .Replace("https://", "")
                .Replace("http://", "")],
            OutdoorAreaIds = [outdoorArea.Id]
        };

        var handler = new UpdateSectorCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var updated = await _dbContext.Sectors
            .AsNoTracking()
            .Include(item => item.OutdoorAreas)
            .SingleAsync(item => item.Id == sector.Id, TestContext.Current.CancellationToken);

        // todo add assertions to verify the response content
        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.Coordinates, updated.Coordinates);
        Assert.True(updated.IsPublic);
        Assert.Single(updated.Media);
        var item = Assert.Single(updated.OutdoorAreas);
        Assert.Equal(outdoorArea.Id, item.Id);
        Assert.Equal(command.Version + 1, updated.Version);
    }
}
