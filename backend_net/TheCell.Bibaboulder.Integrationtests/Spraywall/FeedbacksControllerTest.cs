using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class FeedbacksControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Feedbacks";
    private readonly Faker _bogus;

    public FeedbacksControllerTest(IntegrationTestFactory factory) : base(factory)
    {
        _bogus = new Faker("de_CH");
    }

    [Fact]
    public async Task SendFeedback_Anonymous_Unauthorized()
    {
        var body = new { Feedback = "Some feedback" };

        var response = await Client().PostAsync(
            $"{_baseUrl}/send",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SendFeedback_Authenticated_Ok()
    {
        var user = new UserBuilder().Build();
        await BiBaBoulderDbContext.InsertEntityAndSaveChangesAsync(user);

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: AuthorizationRoles.User,
            username: user.Username);

        var feedbackText = _bogus.Lorem.Sentence();
        var body = new { Feedback = feedbackText };
        var timestamp = DateTime.UtcNow;

        var response = await client.PostAsync(
            $"{_baseUrl}/send",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        Assert.Equal(1, EmailService.SendCount);
        Assert.Equal(user.Email, EmailService.LastRecipient);
        Assert.Contains(feedbackText, EmailService.LastBody);

        var savedMail = await BiBaBoulderDbContext.Mails
            .Where(m => m.To == user.Email)
            .FirstOrDefaultAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedMail);
        Assert.Equal(user.Email, savedMail.To);
        Assert.Contains(user.Email, savedMail.Subject);
        Assert.Contains(feedbackText, savedMail.Body);
        Assert.True(savedMail.SentAt > timestamp);
    }
}
