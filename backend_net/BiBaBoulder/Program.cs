using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thecell.Bibaboulder.BiBaBoulder.Extensions;
using Thecell.Bibaboulder.BiBaBoulder.Middleware;
using Thecell.Bibaboulder.Model;

namespace Thecell.Bibaboulder.BiBaBoulder;

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
        builder.Services.RegisterCqrsAssemblies();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
