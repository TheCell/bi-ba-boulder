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

public class CreateOutdoorAreaTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Faker _bogus;

    public CreateOutdoorAreaTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateOutdoorArea_Anonymous_NotFoundException()
    {
        var command = new CreateOutdoorAreaCommand { Name = _bogus.Lorem.Slug() };
        var handler = new CreateOutdoorAreaCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateOutdoorArea_Ok()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.ContentAdmin)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var sector = new SectorBuilder()
            .SetName("The Shield")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(sector);

        var command = new CreateOutdoorAreaCommand
        {
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            ImportantInfo = _bogus.Lorem.Sentence(),
            PreviewImageUri = _bogus.Internet.Url(),
            ImageUris = [_bogus.Internet.Url()],
            SectorIds = [sector.Id]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateOutdoorAreaCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var outdoorArea = await _dbContext.OutdoorAreas
            .AsNoTracking()
            .Include(area => area.Sectors)
            .SingleAsync(area => area.Id == command.Id, TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.Equal(command.Name, outdoorArea.Name);
        Assert.Single(outdoorArea.Media);
        Assert.Single(outdoorArea.Sectors);
        Assert.Equal(sector.Id, outdoorArea.Sectors.Single().Id);
        Assert.Equal(1, outdoorArea.Version);
    }

    [Fact]
    public async Task CreateOutdoorArea_UnknownSector_ArgumentException()
    {
        var user = new UserBuilder().SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateOutdoorAreaCommand
        {
            Name = _bogus.Lorem.Slug(),
            SectorIds = [Guid.CreateVersion7()]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateOutdoorAreaCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
        Assert.Equal("One or more sectors do not exist.", ex.Message);
    }
}
