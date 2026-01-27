using Microsoft.EntityFrameworkCore;
using Model.Model;

namespace BiBaBoulder.Model
{
    public class BiBaBoulderDbContext : DbContext
      {
        public BiBaBoulderDbContext(DbContextOptions<BiBaBoulderDbContext> options): base(options)
        { }

        public DbSet<Spraywall> Spraywalls { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //  modelBuilder.Entity<Spraywall>()
        //    .HasKey(s => s.Id);
        //}
      }
}
