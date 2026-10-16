using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thecell.Bibaboulder.Model;

namespace TheCell.Bibaboulder.Sharedtests;

/// <summary>
/// A test authentication handler that reads a test subject from request headers
/// and then enriches the principal from the database to mimic production auth.
///
/// Usage in tests:
///   - Set "X-Test-SubjectId" header to simulate an authenticated user
///   - Seed a matching User.OidcSubject to get db_user_id, username and roles from the database
///   - Optionally set fallback headers when no matching local user exists
///   - Omit all headers to simulate an anonymous request
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string SubjectIdHeader = "X-Test-SubjectId";
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
        if (!TryGetSubjectId(out var subjectId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var dbContext = Context.RequestServices.GetRequiredService<IBiBaBoulderDbContext>();
        var user = dbContext.Users.SingleOrDefault(u => u.OidcSubject == subjectId);

        var username = user?.Username
            ?? (Request.Headers.TryGetValue(UsernameHeader, out var usernameValue)
                ? usernameValue.ToString()
                : "testuser");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subjectId),
            new(ClaimTypes.Name, username),
            new("sub", subjectId),
        };

        var email = user?.Email;
        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim("email", email!));
        }

        if (user is not null)
        {
            claims.Add(new Claim("db_user_id", user.Id.ToString()));
            AddRoles(claims, user.Roles);
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryGetSubjectId(out string subjectId)
    {
        if (Request.Headers.TryGetValue(SubjectIdHeader, out var subjectValue))
        {
            subjectId = subjectValue.ToString();
            return true;
        }

        subjectId = string.Empty;
        return false;
    }

    private static void AddRoles(List<Claim> claims, string roles)
    {
        foreach (var role in roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }
}
