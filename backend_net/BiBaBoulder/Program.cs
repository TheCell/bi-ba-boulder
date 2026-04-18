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
using Microsoft.OpenApi;

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
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var config = context.ApplicationServices.GetRequiredService<IConfiguration>();
                document.Info.Title = "BiBaBoulder API";
                document.Info.Version = "v1";
                document.Servers = [new OpenApiServer { Url = config["ApiUrl"] ?? "http://localhost:5088" }];
                return Task.CompletedTask;
            });
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var routeValues = context.Description.ActionDescriptor.RouteValues;
                if (routeValues.TryGetValue("action", out var action) && !string.IsNullOrEmpty(action))
                {
                    operation.OperationId = char.ToLowerInvariant(action[0]) + action[1..];
                }
                return Task.CompletedTask;
            });
        });
        builder.Services.AddDbContext<BiBaBoulderDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("BiBaBoulderDatabase")));
        builder.Services.AddScoped<IBiBaBoulderDbContext>(provider => provider.GetRequiredService<BiBaBoulderDbContext>());

        builder.Services.AddControllers();
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddSingleton<IFileStorageClient, LocalFileStorageClient>();
        builder.Services.AddScoped<ISpraywallImageService, SpraywallImageService>();
        builder.Services.AddScoped<IEmailService, NoOpEmailService>();
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
            // Production: Require HTTPS with strict settings
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;

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
                            Roles = $"{AuthorizationRoles.User},{AuthorizationRoles.Editor}", // OIDC users are auto-verified, grant editor access
                            IsVerified = true,
                        };
                        dbContext.Users.Add(user);
                        await dbContext.SaveChangesAsync(ctx.HttpContext.RequestAborted);
                    }

                    // Add the DB user ID and role(s) as claims so [Authorize(Roles=...)] works
                    // and the DbContext can stamp audit fields without a DB round-trip.
                    if (ctx.Principal?.Identity is ClaimsIdentity identity)
                    {
                        identity.AddClaim(new Claim("db_user_id", user.Id.ToString()));

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
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AntiforgeryMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
