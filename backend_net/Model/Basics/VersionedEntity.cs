using System.ComponentModel.DataAnnotations;

namespace Thecell.Bibaboulder.Model.Basics;

public abstract class VersionedEntity : AuditableEntity
{
    [ConcurrencyCheck]
    public long Version { get; set; }
}

