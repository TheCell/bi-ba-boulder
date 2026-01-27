using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BiBaBoulder.Model;

public class BiBaBoulderDbContextFactory : IDesignTimeDbContextFactory<BiBaBoulderDbContext>
{
    public BiBaBoulderDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BiBaBoulderDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("BiBaBoulderDatabase"));

        return new BiBaBoulderDbContext(optionsBuilder.Options);
    }
}
