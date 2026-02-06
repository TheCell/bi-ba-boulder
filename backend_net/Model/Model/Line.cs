using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class Line : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(9)]
    public string? Color { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public required string Identifier { get; set; }

    public string? Description { get; set; }

    [ForeignKey(nameof(Bloc))]
    public required Guid BlocId { get; set; }

    public Bloc Bloc { get; set; } = null!;

    public ICollection<Point> Points { get; set; } = [];
}
