using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model.Outdoor;

public class Bloc : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Coordinates { get; set; }

    [MaxLength(2048)]
    public string? BlocLowRes { get; set; }

    [MaxLength(2048)]
    public string? BlocMedRes { get; set; }

    [MaxLength(2048)]
    public string? BlocHighRes { get; set; }

    [ForeignKey(nameof(Sector))]
    public Guid? SectorId { get; set; }

    public Sector? Sector { get; set; }

    public ICollection<Line> BoulderLines { get; set; } = [];

    public ICollection<Bloc> AdditionalParts { get; set; } = [];
}
