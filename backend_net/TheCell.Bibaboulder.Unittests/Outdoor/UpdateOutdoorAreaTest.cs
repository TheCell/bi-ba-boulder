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

public class UpdateOutdoorAreaTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserService;
    private readonly Faker _bogus;

    public UpdateOutdoorAreaTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserService = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task UpdateOutdoorArea_NotFound_NotFoundException()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var command = new UpdateOutdoorAreaCommand { Id = Guid.CreateVersion7(), Name = _bogus.Lorem.Slug(), Version = 1 };
        var handler = new UpdateOutdoorAreaCommandHandler(_dbContext, _currentUserService);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"OutdoorArea not found. (Id: {command.Id})", ex.Message);
    }

    [Fact]
    public async Task UpdateOutdoorArea_UnknownSector_ArgumentException()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var outdoorArea = new OutdoorAreaBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new UpdateOutdoorAreaCommand
        {
            Id = outdoorArea.Id,
            Version = outdoorArea.Version,
            Name = outdoorArea.Name,
            SectorIds = [Guid.CreateVersion7()]
        };

        var handler = new UpdateOutdoorAreaCommandHandler(_dbContext, _currentUserService);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
        Assert.Equal("One or more sectors do not exist.", ex.Message);
    }

    [Fact]
    public async Task UpdateOutdoorArea_ReplacesSectorsAndImages_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var originalSector = new SectorBuilder()
            .SetName("Original Sector")
            .Build();
        var newSector = new SectorBuilder()
            .SetName("New Sector")
            .Build();
        await _dbContext.InsertEntitiesAndSaveChangesAsync([originalSector, newSector]);

        var outdoorArea = new OutdoorAreaBuilder()
            .SetName("Original")
            .SetSectors([originalSector])
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new UpdateOutdoorAreaCommand
        {
            Id = outdoorArea.Id,
            Version = outdoorArea.Version,
            Name = _bogus.Lorem.Slug(),
            ImageUris = [_bogus.Internet.Url()
                .Replace("https://", "")
                .Replace("http://", "")],
            SectorIds = [newSector.Id]
        };

        var handler = new UpdateOutdoorAreaCommandHandler(_dbContext, _currentUserService);
        await handler.HandleAsync(command);

        var updated = await _dbContext.OutdoorAreas
            .AsNoTracking()
            .Include(area => area.Sectors)
            .SingleAsync(area => area.Id == outdoorArea.Id, TestContext.Current.CancellationToken);

        // todo add assertions to verify the response content
        Assert.Equal(command.Name, updated.Name);
        Assert.Single(updated.Sectors);
        Assert.Equal(newSector.Id, updated.Sectors.Single().Id);
        Assert.Single(updated.Media);
        Assert.Equal(command.Version + 1, updated.Version);
    }
}
