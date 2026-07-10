using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model;

public interface IBiBaBoulderDbContext : IDisposable
{
    DatabaseFacade Database { get; }

    DbSet<User> Users { get; set; }
    DbSet<Spraywall> Spraywalls { get; set; }
    DbSet<SpraywallProblem> SpraywallProblems { get; set; }
    DbSet<BoulderLog> BoulderLogs { get; set; }
    DbSet<LogEntry> LogEntries { get; set; }
    DbSet<Sector> Sectors { get; set; }
    DbSet<Bloc> Blocs { get; set; }
    DbSet<Line> Lines { get; set; }
    DbSet<Email> Emails { get; set; }

    Task InsertEntityAndSaveChangesAsync(VersionedEntity entity);

    Task InsertEntityAndSaveChangesAsync(EntityAuditFields entity);

    Task InsertEntitiesAndSaveChangesAsync(IEnumerable<VersionedEntity> entities);

    Task InsertEntitiesAndSaveChangesAsync(IEnumerable<EntityAuditFields> entities);

    Task UpdateEntityAndSaveChangesAsync(VersionedEntity entityFromDb, long version);

    Task RemoveEntityAndSaveChangesAsync(VersionedEntity entityFromDb, long version);

    Task RemoveEntityAndSaveChangesAsync(EntityAuditFields entityFromDb);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
