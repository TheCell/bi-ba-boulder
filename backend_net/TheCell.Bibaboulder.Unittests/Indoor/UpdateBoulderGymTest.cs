using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Indoor;

public class UpdateBoulderGymTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserService;
    private readonly Faker _bogus;

    public UpdateBoulderGymTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserService = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task UpdateBoulderGym_NotFound_NotFoundException()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var command = new UpdateBoulderGymCommand { Id = Guid.CreateVersion7(), Name = _bogus.Lorem.Slug(), Version = 1 };
        var handler = new UpdateBoulderGymCommandHandler(_dbContext, _currentUserService);

        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
        Assert.Equal($"BoulderGym not found. (Id: {command.Id})", ex.Message);
    }

    [Fact]
    public async Task UpdateBoulderGym_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var boulderGym = new BoulderGymBuilder()
            .SetName("Original")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand
        {
            Id = boulderGym.Id,
            Version = boulderGym.Version,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            ImportantInfo = _bogus.Lorem.Sentence(),
            PreviewImageUri = _bogus.Internet.Url()
            .Replace("https://", "")
            .Replace("http://", ""),
            ImageUris = [_bogus.Internet.Url()
            .Replace("https://", "")
            .Replace("http://", "")]
        };

        var handler = new UpdateBoulderGymCommandHandler(_dbContext, _currentUserService);
        await handler.HandleAsync(command);

        var updated = await _dbContext.BoulderGyms
            .Include(gym => gym.Media)
            .AsNoTracking()
            .SingleAsync(gym => gym.Id == boulderGym.Id, TestContext.Current.CancellationToken);

        Assert.Equal(command.Name, updated.Name);
        Assert.Equal(command.Description, updated.Description);
        Assert.Equal(command.ImportantInfo, updated.ImportantInfo);
        Assert.Equal(command.PreviewImageUri, updated.PreviewImageUri);
        Assert.Single(updated.Media);
        Assert.Equal(command.ImageUris.Single(), updated.Media.Single().Uri);
        Assert.Equal(command.Version + 1, updated.Version);
    }

    [Fact]
    public async Task UpdateBoulderGym_()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var boulderGym = new BoulderGymBuilder()
            .SetName("Original")
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand
        {
            Id = boulderGym.Id,
            Version = boulderGym.Version,
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            ImportantInfo = _bogus.Lorem.Sentence(),
            PreviewImageUri = _bogus.Internet.Url(),
            ImageUris = [_bogus.Internet.Url()]
        };

        var handler = new UpdateBoulderGymCommandHandler(_dbContext, _currentUserService);
        await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateBoulderGym_ReplacesExistingImages_Ok()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var boulderGym = new BoulderGymBuilder()
            .SetName("Original")
            .SetImages([new PublicResourceBuilder().SetUri("https://example.com/old.jpg").SetResourceType(Thecell.Bibaboulder.Model.Enums.ResourceType.Image).Build()])
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand
        {
            Id = boulderGym.Id,
            Version = boulderGym.Version,
            Name = boulderGym.Name,
            ImageUris = ["example/new.jpg"]
        };

        var handler = new UpdateBoulderGymCommandHandler(_dbContext, _currentUserService);
        await handler.HandleAsync(command);

        var updated = await _dbContext.BoulderGyms
            .AsNoTracking()
            .SingleAsync(gym => gym.Id == boulderGym.Id, TestContext.Current.CancellationToken);

        Assert.Single(updated.Media);
        Assert.Equal("example/new.jpg", updated.Media.Single().Uri);
    }

    [Fact]
    public async Task UpdateBoulderGym_InvalidImageUri_ArgumentException()
    {
        var contentAdmin = new UserBuilder().SetUsername("Content Admin").SetRoles(AuthorizationRoles.ContentAdmin).Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(contentAdmin);
        _currentUserService.WithUser(contentAdmin);

        var boulderGym = new BoulderGymBuilder().Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);

        var command = new UpdateBoulderGymCommand
        {
            Id = boulderGym.Id,
            Version = boulderGym.Version,
            Name = boulderGym.Name,
            ImageUris = ["ftp://example.com/image.jpg"]
        };

        var handler = new UpdateBoulderGymCommandHandler(_dbContext, _currentUserService);

        await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
    }
}
