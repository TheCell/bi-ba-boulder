using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Common.Appsettings;
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
    private readonly IOptions<EmailSettings> _emailSettings;

    public SendFeedbackTest()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _dbContext = new DbContextMock().Build();

        var services = new ServiceCollection();
        services.AddScoped(_ => _dbContext);
        _serviceProvider = services.BuildServiceProvider();

        _emailService = new DatabaseEmailService(_serviceProvider);
        _emailSettings = Options.Create(new EmailSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            UseSsl = false,
            UseStartTls = true,
            Username = "test@example.com",
            Password = "password",
            ContactAddress = "contact@bibaboulder.com",
            FromAddress = "noreply@bibaboulder.com",
            FromName = "Bi Ba Boulder"
        });
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

        var handler = new SendFeedbackCommandHandler(_currentUserServiceMock.Object, _emailService, _emailSettings);
        await handler.HandleAsync(command);

        Assert.Equal(1, _emailService.SendCount);
        Assert.Equal("contact@bibaboulder.com", _emailService.LastRecipient);
        Assert.Contains("Great app!", _emailService.LastBody);
    }

    [Fact]
    public async Task SendFeedback_EmptyFeedback_ThrowsArgumentException()
    {
        var command = new SendFeedbackCommand
        {
            Feedback = "   "
        };

        var handler = new SendFeedbackCommandHandler(_currentUserServiceMock.Object, _emailService, _emailSettings);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await handler.HandleAsync(command));
    }
}
