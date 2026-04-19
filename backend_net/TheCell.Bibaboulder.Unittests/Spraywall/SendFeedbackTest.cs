using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Unittests.Spraywall;

public class SendFeedbackTest
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DatabaseEmailService _emailService;
    private readonly User _testUser;
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly IServiceProvider _serviceProvider;

    public SendFeedbackTest()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dbContext = new DbContextMock().Build();

        var services = new ServiceCollection();
        services.AddScoped(_ => _dbContext);
        _serviceProvider = services.BuildServiceProvider();

        _emailService = new DatabaseEmailService(_serviceProvider);
        _testUser = new UserBuilder().SetEmail("feedback@test.com").Build();
        _currentUserServiceMock.Setup(s => s.GetCurrentUserOrThrowAsync()).ReturnsAsync(_testUser);
    }

    [Fact]
    public async Task SendFeedback_Ok()
    {
        var command = new SendFeedbackCommand
        {
            Feedback = "Great app!"
        };

        var handler = new SendFeedbackCommandHandler(_currentUserServiceMock.Object, _emailService);
        await handler.HandleAsync(command);

        Assert.Equal(1, _emailService.SendCount);
        Assert.Equal("feedback@test.com", _emailService.LastRecipient);
        Assert.Contains("Great app!", _emailService.LastBody);
    }

    [Fact]
    public async Task SendFeedback_EmptyFeedback_ThrowsArgumentException()
    {
        var command = new SendFeedbackCommand
        {
            Feedback = "   "
        };

        var handler = new SendFeedbackCommandHandler(_currentUserServiceMock.Object, _emailService);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
    }
}
