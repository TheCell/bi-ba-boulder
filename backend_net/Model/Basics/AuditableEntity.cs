using System;

namespace Thecell.Bibaboulder.Model.Basics;

public abstract class AuditableEntity
{
    public required DateTime CreatedDate { get; set; }

    public required Guid CreatedUserId { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public Guid? UpdatedUserId { get; set; }

    public DateTime? DeletedDate { get; set; }

    public Guid? DeletedUserId { get; set; }
}
