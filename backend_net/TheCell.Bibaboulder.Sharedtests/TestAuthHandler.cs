using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TheCell.Bibaboulder.Sharedtests;

/// <summary>
/// A test authentication handler that reads claims from request headers.
/// This allows integration tests to simulate authenticated requests without
/// requiring a real OIDC Identity Provider.
///
/// Usage in tests:
///   - Set "X-Test-UserId" header to simulate an authenticated user
///   - Set "X-Test-Role" header to assign roles (comma-separated for multiple roles)
///   - Set "X-Test-Username" header to set the user's name claim
///   - Omit all headers to simulate an anonymous request
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-Role";
    public const string UsernameHeader = "X-Test-Username";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // If no test user header is present, return no result (anonymous)
        if (!Request.Headers.TryGetValue(UserIdHeader, out var value))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = value.ToString();
        var username = Request.Headers.TryGetValue(UsernameHeader, out var value1)
            ? value1.ToString() : "testuser";

        var claims = new System.Collections.Generic.List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, username),
            new("sub", userId),
        };

        // Add role claims if present
        if (Request.Headers.TryGetValue(RoleHeader, out var value2))
        {
            var roles = value2.ToString().Split(',');
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
