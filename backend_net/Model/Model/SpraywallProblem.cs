using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Model;

public class SpraywallProblem : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(512)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public FontGrade? FontGrade { get; set; }

    [ForeignKey(nameof(Spraywall))]
    public required Guid SpraywallId { get; set; }

    public Spraywall Spraywall { get; set; } = null!;

    public ICollection<BoulderLog> BoulderLogs { get; set; } = [];
}
