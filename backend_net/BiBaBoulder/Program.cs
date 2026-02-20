using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Thecell.Bibaboulder.BiBaBoulder.Extensions;
using Thecell.Bibaboulder.BiBaBoulder.Middleware;
using Thecell.Bibaboulder.Model;

namespace Thecell.Bibaboulder.BiBaBoulder;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506: Avoid excessive class coupling", Justification = "This is the Main Method, we expect excessive class coupings")]
public class Program
{
    public IConfiguration Configuration { get; }

    public Program(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<BiBaBoulderDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("BiBaBoulderDatabase")));
        builder.Services.AddScoped<IBiBaBoulderDbContext>(provider => provider.GetRequiredService<BiBaBoulderDbContext>());

        builder.Services.AddControllers();
        builder.Services.RegisterCqrsAndControllerAssemblies();

        // Authentication: Cookie (default) + OpenID Connect (challenge)
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax; // Lax required for OIDC redirect back from IdP
            options.Cookie.Name = "BiBaBoulder.Auth";

            // Return 401 for API/BFF calls instead of redirecting to the IdP login page
            options.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") ||
                    ctx.Request.Path.StartsWithSegments("/bff"))
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") ||
                    ctx.Request.Path.StartsWithSegments("/bff"))
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };
        })
        .AddOpenIdConnect(options =>
        {
            options.Authority = builder.Configuration["Oidc:Authority"];
            options.ClientId = builder.Configuration["Oidc:ClientId"];
            options.ClientSecret = builder.Configuration["Oidc:ClientSecret"];
            options.ResponseType = OpenIdConnectResponseType.Code; // Authorization Code flow
            options.UsePkce = true;
            options.SaveTokens = true; // Store tokens server-side in the auth cookie
            options.GetClaimsFromUserInfoEndpoint = true;
            options.MapInboundClaims = false; // Preserve claim names from IdP

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            // Map the IdP's role claim to ClaimTypes so [Authorize(Roles=...)] works.
            // Adjust "roles" to match your IdP's role claim name (e.g. "realm_access" for Keycloak).
            options.TokenValidationParameters.RoleClaimType = "roles";
        });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AntiforgeryMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
