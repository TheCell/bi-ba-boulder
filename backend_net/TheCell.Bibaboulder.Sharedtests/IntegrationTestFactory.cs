using System.Data.Common;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Thecell.Bibaboulder.BiBaBoulder.Extensions;
using Thecell.Bibaboulder.BiBaBoulder.Middleware;
using Thecell.Bibaboulder.Model;

namespace TheCell.Bibaboulder.Sharedtests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506: Avoid excessive class coupling", Justification = "This is the Testfactory, we expect excessive class coupings")]
public class IntegrationTestFactory : WebApplicationFactory<EntryPoint>
{
    private readonly DbConnection _connection;
    private readonly string _connectionString = "DataSource=:memory:";

    public IntegrationTestFactory()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            var configuration = context.Configuration;
            //services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

            // Add SQLite in-memory database for testing
            services.AddEntityFrameworkSqlite();
            services.AddDbContext<BiBaBoulderDbContext>((options) =>
            {
                options.UseSqlite(_connection);

                //options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning)); // Ambient Transactions are not supported in SQLite
                //options.ConfigureWarnings(x => x.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables));
            });

            // Ensure IBiBaBoulderDbContext is registered
            services.AddScoped<IBiBaBoulderDbContext>(c => c.GetRequiredService<BiBaBoulderDbContext>());

            services.AddControllers()
                .ConfigureApiBehaviorOptions((options) =>
                {
                    options.InvalidModelStateResponseFactory = (context) =>
                    {
                        var errors = context.ModelState
                            .Where(e => e.Value?.Errors.Count > 0)
                            .Select(e => new
                            {
                                Field = e.Key,
                                Errors = e.Value?.Errors.Select(er => er.ErrorMessage).ToArray()
                            }).ToArray();

                        return new BadRequestObjectResult(new { Errors = errors });
                    };
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve property names as defined in C# classes
                });

            services.RegisterCqrsAssemblies();

            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<IBiBaBoulderDbContext>();
            dbContext!.Database.EnsureCreated();
        }).Configure((app) =>
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });

    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Close();
    }
}
