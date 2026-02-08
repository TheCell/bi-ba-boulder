using System.ComponentModel.DataAnnotations;

namespace Thecell.Bibaboulder.Model.Basics;

public abstract class VersionedEntity : EntityAuditFields
{
    [ConcurrencyCheck]
    public long Version { get; set; }
}

