using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Model;

public class Line : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// This is the name of the route. Ex. Veni, Vidi, Vici
    /// </summary>
    [MaxLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// This is the Routes Number or Identifier, Ex. 1, 2, A, B etc.
    /// </summary>
    /// <remarks>
    /// This can be used to refer to other routes in the description. For Example exit over Route 7 is a 7C+
    /// </remarks>
    [MaxLength(255)]
    public required string Identifier { get; set; }

    public FontGrade? FontGrade { get; set; }

    public string? Description { get; set; }

    [ForeignKey(nameof(Bloc))]
    public required Guid BlocId { get; set; }

    public Bloc Bloc { get; set; } = null!;

    /// <summary>
    /// Json data for the route
    /// </summary>
    public required LineData Data { get; set; }
}
