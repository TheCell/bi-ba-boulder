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
using TheCell.Bibaboulder.Sharedtests.Assertions;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Outdoor;

public class CreateSectorTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Faker _bogus;

    public CreateSectorTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateSector_Anonymous_AccessDeniedException()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateSectorCommand
        {
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph()
        };

        var handler = new CreateSectorCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () =>
            await handler.HandleAsync(command));
        Assert.Equal("No current user configured in mock.", ex.Message);
    }

    [Fact]
    public async Task CreateSector_Ok()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateSectorCommand
        {
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            ImportantInfo = _bogus.Lorem.Sentence(),
            IsPublic = _bogus.Random.Bool(),
            Coordinates = "46.9914628, 7.5589870",
            PreviewImageUri = _bogus.Internet.Url()
                .Replace("https://", "")
                .Replace("http://", "")
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSectorCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var sector = await _dbContext.Sectors
            .AsNoTracking()
            .SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        SectorAssertion.Assert(command, sector);
        Assert.Equal(1, sector.Version);
    }

    [Fact]
    public async Task CreateSector_UnknownOutdoorArea_ArgumentException()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateSectorCommand
        {
            Name = _bogus.Lorem.Slug(),
            OutdoorAreaIds = [Guid.NewGuid()]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSectorCommandHandler(_dbContext, _currentUserServiceMock);

        var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
        Assert.Equal("One or more outdoor areas do not exist.", ex.Message);
    }

    [Fact]
    public async Task CreateSector_WithOutdoorArea_Ok()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.Editor)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var outdoorArea = new OutdoorAreaBuilder().SetName("Lindental").Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(outdoorArea);

        var command = new CreateSectorCommand
        {
            Name = _bogus.Lorem.Slug(),
            OutdoorAreaIds = [outdoorArea.Id]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateSectorCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var sector = await _dbContext.Sectors
            .AsNoTracking()
            .Include(item => item.OutdoorAreas)
            .SingleAsync(item => item.Id == command.Id, TestContext.Current.CancellationToken);
        Assert.Single(sector.OutdoorAreas);
        Assert.Equal(outdoorArea.Id, sector.OutdoorAreas.Single().Id);
    }
}
