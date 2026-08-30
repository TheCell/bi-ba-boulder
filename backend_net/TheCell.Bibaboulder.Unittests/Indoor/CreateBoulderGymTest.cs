using System;
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

public class CreateBoulderGymTest
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly CurrentUserServiceMock _currentUserServiceMock;
    private readonly Faker _bogus;

    public CreateBoulderGymTest()
    {
        _dbContext = new DbContextMock().Build();
        _currentUserServiceMock = new CurrentUserServiceMock();
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task CreateBoulderGym_Anonymous_NotFoundException()
    {
        var command = new CreateBoulderGymCommand { Name = _bogus.Lorem.Slug() };
        var handler = new CreateBoulderGymCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<NotFoundException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateBoulderGym_Ok()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.ContentAdmin)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateBoulderGymCommand
        {
            Name = _bogus.Lorem.Slug(),
            Description = _bogus.Lorem.Paragraph(),
            ImportantInfo = _bogus.Lorem.Sentence(),
            PreviewImageUri = _bogus.Internet.Url(),
            ImageUris = [_bogus.Internet.Url(), _bogus.Internet.Url()]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateBoulderGymCommandHandler(_dbContext, _currentUserServiceMock);
        await handler.HandleAsync(command);

        var boulderGym = await _dbContext.BoulderGyms
            .AsNoTracking()
            .SingleAsync(gym => gym.Id == command.Id, TestContext.Current.CancellationToken);

        // todo add assertertions to verify the response content
        Assert.Equal(command.Name, boulderGym.Name);
        Assert.Equal(command.Description, boulderGym.Description);
        Assert.Equal(command.ImportantInfo, boulderGym.ImportantInfo);
        Assert.Equal(command.PreviewImageUri, boulderGym.PreviewImageUri);
        Assert.Equal(command.ImageUris.Count, boulderGym.Media.Count);
        Assert.Equal(1, boulderGym.Version);
        Assert.Equal(user.Id, boulderGym.CreatedUserId);
    }

    [Fact]
    public async Task CreateBoulderGym_InvalidImageUri_ArgumentException()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.ContentAdmin)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var command = new CreateBoulderGymCommand
        {
            Name = _bogus.Lorem.Slug(),
            ImageUris = ["not-a-valid-url"]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateBoulderGymCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateBoulderGym_DuplicateImageUri_ArgumentException()
    {
        var user = new UserBuilder()
            .SetRoles(AuthorizationRoles.ContentAdmin)
            .Build();
        await _dbContext.InsertEntityAndSaveChangesAsync(user);

        var imageUri = _bogus.Internet.Url();
        var command = new CreateBoulderGymCommand
        {
            Name = _bogus.Lorem.Slug(),
            ImageUris = [imageUri, imageUri]
        };

        _currentUserServiceMock.WithUser(user);
        var handler = new CreateBoulderGymCommandHandler(_dbContext, _currentUserServiceMock);

        await Assert.ThrowsAsync<ArgumentException>(async () => await handler.HandleAsync(command));
    }
}
