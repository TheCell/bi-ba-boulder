using System;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Model.Access;

public class UserSectorAccess : VersionedEntity
{
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required User User { get; set; }

    public Guid SectorId { get; set; }

    [ForeignKey(nameof(SectorId))]
    public required Sector Sector { get; set; }

    public AccessSourceType AccessSourceType { get; set; }

    public DateTime? ValidUntil { get; set; }
}
