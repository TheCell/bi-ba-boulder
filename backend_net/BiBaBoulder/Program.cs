using System;
using System.Linq;
using System.Security.Claims;
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
using Thecell.Bibaboulder.BiBaBoulder.Services;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.RegisterCqrsAndControllerAssemblies();

        var frontendOrigin = builder.Configuration["FrontendOrigin"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("FrontendOrigin is not configured in appsettings.");
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(frontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

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

            // Google doesn't provide role claims, so we use ClaimTypes.Role
            // and enrich it from the database in OnTokenValidated below.
            options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = async ctx =>
                {
                    var sub = ctx.Principal?.FindFirstValue("sub");
                    var email = ctx.Principal?.FindFirstValue("email");
                    var name = ctx.Principal?.FindFirstValue("name")
                               ?? ctx.Principal?.FindFirstValue("preferred_username")
                               ?? email;

                    if (string.IsNullOrEmpty(sub))
                    {
                        return;
                    }

                    // Look up or create the local user record
                    var dbContext = ctx.HttpContext.RequestServices
                        .GetRequiredService<IBiBaBoulderDbContext>();
                    var user = await dbContext.Users
                        .FirstOrDefaultAsync(
                            u => u.OidcSubject == sub,
                            ctx.HttpContext.RequestAborted);

                    if (user is null)
                    {
                        user = new User
                        {
                            Id = Guid.NewGuid(),
                            OidcSubject = sub,
                            Email = email ?? "",
                            Username = name ?? "",
                            Roles = AuthorizationRoles.User, // default role for new users
                            IsVerified = true,
                        };
                        dbContext.Users.Add(user);
                        await dbContext.SaveChangesAsync(ctx.HttpContext.RequestAborted);
                    }

                    // Add the DB role(s) as claims so [Authorize(Roles=...)] works
                    if (ctx.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var roles = user.Roles.Split(',',
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (var role in roles)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                    }
                },
            };
        });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AntiforgeryMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
