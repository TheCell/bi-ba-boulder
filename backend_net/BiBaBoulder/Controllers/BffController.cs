using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Thecell.Bibaboulder.BiBaBoulder.Controllers;

/// <summary>
/// Backend-for-Frontend (BFF) session management endpoints.
/// These endpoints manage the OIDC authentication lifecycle for the Angular frontend.
/// The frontend never handles tokens directly; all auth state is managed server-side via cookies.
/// </summary>
[ApiController]
[Route("[controller]")]
public class BffController : ControllerBase
{
    /// <summary>
    /// Initiates the OIDC Authorization Code + PKCE flow by redirecting to the Identity Provider.
    /// The Angular app should navigate to this URL (not call it via AJAX).
    /// </summary>
    /// <param name="returnUrl">URL to redirect back to after successful login. Defaults to "/".</param>
    [HttpGet("login")]
    [AllowAnonymous]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1054: URI parameters should not be strings", Justification = "We accept this exception")]
    public IActionResult Login(string? returnUrl = "/")
    {
        // Prevent open redirect attacks by only allowing local return URLs
        if (!Url.IsLocalUrl(returnUrl))
        {
            returnUrl = "/";
        }

        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl });
    }

    /// <summary>
    /// Returns the current user's claims as JSON.
    /// The Angular app calls this to check if a session is active and to retrieve user info.
    /// Returns 401 if the user is not authenticated.
    /// </summary>
    [HttpGet("user")]
    [Authorize]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(" Naming", "CA1721: Property names should not match get methods", Justification = "This is a special case for the BFF pattern")]
    public IActionResult GetUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    /// <summary>
    /// Signs out of the local cookie session and the OIDC Identity Provider.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }
}
