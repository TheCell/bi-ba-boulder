using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Thecell.Bibaboulder.BiBaBoulder.Middleware;

/// <summary>
/// Middleware that protects against CSRF attacks by requiring a custom X-CSRF header on API requests.
///
/// In the BFF pattern, authentication is cookie-based, which means browsers will automatically
/// attach the auth cookie to requests. A CSRF attack can exploit this by triggering requests
/// from a malicious site. Requiring a custom header prevents this because:
/// 1. Browsers won't add custom headers to cross-origin "simple" requests
/// 2. Adding a custom header triggers a CORS preflight check, which will be blocked by the
///    server's CORS policy
///
/// The Angular frontend must include "X-CSRF: 1" on all AJAX requests to /api/* and /bff/* endpoints.
/// The /bff/login endpoint is exempt because it's a browser navigation (redirect), not an AJAX call.
/// </summary>
public class AntiforgeryMiddleware
{
    private readonly RequestDelegate _next;

    public AntiforgeryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (RequiresCsrfProtection(context.Request) && !context.Request.Headers.ContainsKey("X-CSRF"))
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { message = "Missing required X-CSRF header" });
            return;
        }

        await _next(context);
    }

    private static bool RequiresCsrfProtection(HttpRequest request)
    {
        var path = request.Path;

        // API endpoints always require CSRF protection
        if (path.StartsWithSegments("/api"))
        {
            return true;
        }

        // BFF endpoints require CSRF protection, except /bff/login which is a browser navigation
        if (path.StartsWithSegments("/bff") && !path.StartsWithSegments("/bff/login"))
        {
            return true;
        }

        return false;
    }
}
