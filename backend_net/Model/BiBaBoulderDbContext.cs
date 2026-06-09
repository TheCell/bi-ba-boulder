using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model;

public class BiBaBoulderDbContext : DbContext, IBiBaBoulderDbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public BiBaBoulderDbContext(DbContextOptions<BiBaBoulderDbContext> options) : base(options)
    { }

    public BiBaBoulderDbContext(DbContextOptions<BiBaBoulderDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Spraywall> Spraywalls { get; set; }
    public DbSet<SpraywallProblem> SpraywallProblems { get; set; }
    public DbSet<BoulderLog> BoulderLogs { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<Bloc> Blocs { get; set; }
    public DbSet<Line> Lines { get; set; }
    public DbSet<Point> Points { get; set; }
    public DbSet<Email> Emails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // customize the model here if needed
    }

    public async Task InsertEntityAndSaveChangesAsync(VersionedEntity entity)
    {
        entity.Version = 1;
        await InsertEntityAndSaveChangesAsync((EntityAuditFields)entity);
    }

    public async Task InsertEntityAndSaveChangesAsync(EntityAuditFields entity)
    {
        await AddAsync(entity);
        await SaveChangesAsync();
    }

    public async Task InsertEntitiesAndSaveChangesAsync(IEnumerable<VersionedEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.Version = 1;
        }
        await InsertEntitiesAndSaveChangesAsync((IEnumerable<EntityAuditFields>)entities);
    }

    public async Task InsertEntitiesAndSaveChangesAsync(IEnumerable<EntityAuditFields> entities)
    {
        await AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    public async Task UpdateEntityAndSaveChangesAsync(VersionedEntity entityFromDb, long version)
    {
        var entry = Entry(entityFromDb);
        ValidateEntryIsNotDetached(entry);

        entry.Property(nameof(VersionedEntity.Version)).OriginalValue = version;
        entityFromDb.Version = version + 1;

        await SaveChangesAsync();
    }

    public virtual async Task RemoveEntityAndSaveChangesAsync(VersionedEntity entityFromDb, long version)
    {
        var entry = Entry(entityFromDb);
        ValidateEntryIsNotDetached(entry);

        entry.Property(nameof(VersionedEntity.Version)).OriginalValue = version;

        Remove(entityFromDb);

        await SaveChangesAsync();
    }

    public virtual async Task RemoveEntityAndSaveChangesAsync(EntityAuditFields entityFromDb)
    {
        var entry = Entry(entityFromDb);
        ValidateEntryIsNotDetached(entry);

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

    private Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor?.HttpContext?.User?.FindFirst("db_user_id");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    private void SetAuditableFields()
    {
        var currentDate = DateTime.UtcNow;
        var currentUserId = GetCurrentUserId();

        foreach (var entry in ChangeTracker.Entries<EntityAuditFields>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(EntityAuditFields.CreatedDate)).CurrentValue = currentDate;
                entry.Property(nameof(EntityAuditFields.CreatedUserId)).CurrentValue = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(EntityAuditFields.UpdatedDate)).CurrentValue = currentDate;
                entry.Property(nameof(EntityAuditFields.UpdatedUserId)).CurrentValue = currentUserId;
            }
        }
    }
}
