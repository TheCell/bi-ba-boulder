using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Model.Access;
using Thecell.Bibaboulder.Model.Model.Indoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;

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
    public DbSet<Email> Emails { get; set; }
    public DbSet<OutdoorArea> OutdoorAreas { get; set; }
    public DbSet<UserSectorAccess> UserSectorAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        modelBuilder.Entity<Line>()
            .Property(l => l.Data)
            .HasConversion(
                data => JsonSerializer.Serialize(data, options),
                json => JsonSerializer.Deserialize<LineData>(json, options)!
            );

        modelBuilder.Entity<UserSectorAccess>()
            .HasKey(usa => new { usa.UserId, usa.SectorId });

        modelBuilder.Entity<UserSectorAccess>()
            .HasOne(usa => usa.User)
            .WithMany(user => user.UserSectorAccesses)
            .HasForeignKey(usa => usa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSectorAccess>()
            .HasOne(usa => usa.Sector)
            .WithMany()
            .HasForeignKey(usa => usa.SectorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sector>()
            .OwnsMany(sector => sector.Media, media =>
            {
                media.ToTable("SectorImages");
                media.WithOwner().HasForeignKey("SectorId");
                media.Property(resource => resource.Uri).HasMaxLength(2048);
                media.Property(resource => resource.ResourceType).IsRequired();
                media.HasKey("SectorId", "Uri", "ResourceType");
            });

        modelBuilder.Entity<OutdoorArea>()
            .OwnsMany(outdoorArea => outdoorArea.Media, media =>
            {
                media.ToTable("OutdoorAreaImages");
                media.WithOwner().HasForeignKey("OutdoorAreaId");
                media.Property(resource => resource.Uri).HasMaxLength(2048);
                media.Property(resource => resource.ResourceType).IsRequired();
                media.HasKey("OutdoorAreaId", "Uri", "ResourceType");
            });
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
                if ((Guid?)entry.Property(nameof(EntityAuditFields.CreatedUserId)).CurrentValue == Guid.Empty)
                {
                    entry.Property(nameof(EntityAuditFields.CreatedUserId)).CurrentValue = currentUserId;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(EntityAuditFields.UpdatedDate)).CurrentValue = currentDate;
                if ((Guid?)entry.Property(nameof(EntityAuditFields.CreatedUserId)).CurrentValue == Guid.Empty)
                {
                    entry.Property(nameof(EntityAuditFields.UpdatedUserId)).CurrentValue = currentUserId;
                }
            }
        }
    }
}
