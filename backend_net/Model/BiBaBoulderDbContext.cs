using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model;

public class BiBaBoulderDbContext : DbContext, IBiBaBoulderDbContext
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

    public async Task InsertEntityAsync(VersionedEntity entity)
    {
        entity.Version = 1;
        await InsertEntityAsync((EntityAuditFields)entity);
    }

    public async Task InsertEntityAsync(EntityAuditFields entity)
    {
        await AddAsync(entity);
        await SaveChangesAsync();
    }

    public async Task InsertEntitiesAsync(IEnumerable<VersionedEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.Version = 1;
        }
        await InsertEntitiesAsync((IEnumerable<EntityAuditFields>)entities);
    }

    public async Task InsertEntitiesAsync(IEnumerable<EntityAuditFields> entities)
    {
        await AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    public async Task UpdateEntityAsync(VersionedEntity entityFromDb, long version)
    {
        var entry = Entry(entityFromDb);
        ValidateEntryIsNotDetached(entry);

        entry.Property(nameof(VersionedEntity.Version)).OriginalValue = version;
        entityFromDb.Version = version + 1;

        await SaveChangesAsync();
    }

    public virtual async Task RemoveEntityAsync(VersionedEntity entityFromDb, long version)
    {
        var entry = Entry(entityFromDb);
        ValidateEntryIsNotDetached(entry);

        entry.Property(nameof(VersionedEntity.Version)).OriginalValue = version;

        Remove(entityFromDb);

        await SaveChangesAsync();
    }

    public virtual async Task RemoveEntityAsync(EntityAuditFields entityFromDb)
    {
        var entry = Entry(entityFromDb);
        ValidateEntryIsNotDetached(entry);

        Remove(entityFromDb);

        await SaveChangesAsync();
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditableFields();
        // AuditEntities calls ChangeTracker.Entries() which already triggers ChangeDetection. 
        // https://docs.microsoft.com/en-us/ef/core/change-tracking/change-detection#disabling-automatic-change-detection
        ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    private void ValidateEntryIsNotDetached(EntityEntry entry)
    {
        if (entry.State == EntityState.Detached)
        {
            const string message = "Entity is detached. Entities must be loaded from database before removal.";
            throw new InvalidOperationException(message);
        }
    }

    private void SetAuditableFields()
    {
        var currentDate = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityAuditFields>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(EntityAuditFields.CreatedDate)).CurrentValue = currentDate;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(EntityAuditFields.UpdatedDate)).CurrentValue = currentDate;
            }
        }
    }
}
