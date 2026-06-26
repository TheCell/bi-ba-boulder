using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

/// <summary>
/// Development-only authentication endpoints to bypass OIDC for local testing.
/// ONLY AVAILABLE IN DEVELOPMENT MODE.
/// </summary>
[ApiController]
[Route("[controller]")]
public class DevAuthController : ControllerBase
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly IHostEnvironment _environment;

    public DevAuthController(IBiBaBoulderDbContext dbContext, IHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    /// <summary>
    /// Mock login endpoint for development. Creates/uses a test user and signs in with cookies.
    /// </summary>
    /// <param name="email">Email of the test user. Defaults to "dev@test.local".</param>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> DevLogin([FromQuery] string email = "dev@test.local")
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        // Extra security: Only allow localhost requests
        if (!HttpContext.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
            !HttpContext.Request.Host.Host.Equals("127.0.0.1", StringComparison.Ordinal))
        {
            return NotFound();
        }

        // Find or create a dev user
        var oidcSub = $"dev-{email}";
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.OidcSubject == oidcSub);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                OidcSubject = oidcSub,
                Email = email,
                Username = email.Split('@')[0],
                Roles = AuthorizationRoles.User + AuthorizationRoles.Editor + AuthorizationRoles.Admin,
                IsVerified = true,
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        // Build claims identity matching what OIDC would provide
        var claims = new[]
        {
            new Claim("sub", user.OidcSubject),
            new Claim("email", user.Email),
            new Claim("name", user.Username),
            new Claim("db_user_id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, AuthorizationRoles.User),
            new Claim(ClaimTypes.Role, AuthorizationRoles.Editor),
            new Claim(ClaimTypes.Role, AuthorizationRoles.Admin),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
            });

        return Ok(new { message = "Dev login successful", email, userId = user.Id });
    }

    /// <summary>
    /// Clear the development session.
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> DevLogout()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        // Extra security: Only allow localhost requests
        if (!HttpContext.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
            !HttpContext.Request.Host.Host.Equals("127.0.0.1", StringComparison.Ordinal))
        {
            return NotFound();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Dev logout successful" });
    }
}
