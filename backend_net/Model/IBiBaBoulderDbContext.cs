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
    DbSet<Bookmark> Bookmarks { get; set; }
    DbSet<Sector> Sectors { get; set; }
    DbSet<Bloc> Blocs { get; set; }
    DbSet<Line> Lines { get; set; }
    DbSet<Point> Points { get; set; }

    Task InsertEntityAsync(VersionedEntity entity);

    Task InsertEntityAsync(EntityAuditFields entity);

    Task InsertEntitiesAsync(IEnumerable<VersionedEntity> entities);

    Task InsertEntitiesAsync(IEnumerable<EntityAuditFields> entities);

    Task UpdateEntityAsync(VersionedEntity entityFromDb, long version);

    Task RemoveEntityAsync(VersionedEntity entityFromDb, long version);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
