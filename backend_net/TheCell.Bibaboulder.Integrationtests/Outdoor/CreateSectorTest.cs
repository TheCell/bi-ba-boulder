using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace TheCell.Bibaboulder.Integrationtests.Outdoor;

public class CreateSectorTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Index")]
    [InlineData("/About")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange: Create client with the SUT's server
        var client = _factory.CreateClient();

        // Act: Send request to the URL
        var response = await client.GetAsync(url);

        // Assert: Response is successful and has correct Content-Type
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8",
                     response.Content.Headers.ContentType.ToString());
    }
}
