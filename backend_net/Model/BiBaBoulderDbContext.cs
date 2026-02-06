using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model;

public class BiBaBoulderDbContext : DbContext
{
    public BiBaBoulderDbContext(DbContextOptions<BiBaBoulderDbContext> options) : base(options)
    { }

    public DbSet<User> Users { get; set; }
    public DbSet<Spraywall> Spraywalls { get; set; }
    public DbSet<SpraywallProblem> SpraywallProblems { get; set; }
    public DbSet<BoulderLog> BoulderLogs { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<Bloc> Blocs { get; set; }
    public DbSet<Line> Lines { get; set; }
    public DbSet<Point> Points { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // customize the model here if needed
    }
}
