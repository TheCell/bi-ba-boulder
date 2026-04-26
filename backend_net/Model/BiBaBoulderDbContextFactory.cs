using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Thecell.Bibaboulder.Model;

public class BiBaBoulderDbContextFactory : IDesignTimeDbContextFactory<BiBaBoulderDbContext>
{
    public BiBaBoulderDbContext CreateDbContext(string[] args)
    {
        //var configuration = new ConfigurationBuilder()
        //    .SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("appsettings.json")
        //    .AddJsonFile("appsettings.Development.json", optional: true)
        //    .Build();

        //var optionsBuilder = new DbContextOptionsBuilder<BiBaBoulderDbContext>();
        //optionsBuilder.UseSqlServer(configuration.GetConnectionString("BiBaBoulderDatabase"));

        //return new BiBaBoulderDbContext(optionsBuilder.Options);
        var connectionString = @"Server=(localdb)\mssqllocaldb;Database=BiBaBoulder;Trusted_Connection=True;MultipleActiveResultSets=true";

        var dbContextOptionsBuilder = new DbContextOptionsBuilder<BiBaBoulderDbContext>();
        dbContextOptionsBuilder.UseSqlServer(connectionString,
            x => x.MigrationsAssembly("Thecell.Bibaboulder.Migrations"));

        dbContextOptionsBuilder.ConfigureWarnings(
            x => x.Ignore(RelationalEventId.ForeignKeyPropertiesMappedToUnrelatedTables));

        return new BiBaBoulderDbContext(dbContextOptionsBuilder.Options);
    }
}
