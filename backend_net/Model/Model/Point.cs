using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class Point : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    public required double X { get; set; }

    public required double Y { get; set; }

    public required double Z { get; set; }

    [ForeignKey(nameof(Line))]
    public required Guid LineId { get; set; }

    public Line Line { get; set; } = null!;
}
