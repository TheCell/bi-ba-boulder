using System.Net;
using System.Threading.Tasks;
using Bogus;
using Thecell.Bibaboulder.Model.Authorization;
using TheCell.Bibaboulder.Sharedtests;
using TheCell.Bibaboulder.Sharedtests.ModelBuilders;

namespace TheCell.Bibaboulder.Integrationtests.Spraywall;

[Collection(nameof(CollectionForIntegrationTests))]
public class FeedbacksControllerTest : BaseTest
{
    private readonly string _baseUrl = "/api/Feedbacks";
    private readonly Faker _bogus;

    protected FeedbacksControllerTest(IntegrationTestFactory factory) : base(factory)
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
        await BiBaBoulderDbContext.InsertEntityAsync(user);

        var client = AuthenticatedClient(
            userId: user.OidcSubject,
            role: AuthorizationRoles.User,
            username: user.Username);

        var body = new { Feedback = _bogus.Lorem.Sentence() };

        var response = await client.PostAsync(
            $"{_baseUrl}/send",
            GetJsonHttpBody(body),
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
