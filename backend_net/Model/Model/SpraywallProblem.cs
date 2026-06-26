using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.ProblemInterfaces;

namespace Thecell.Bibaboulder.Model.Model;

public class SpraywallProblem : VersionedEntity, IProblemTags
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

    [ForeignKey(nameof(Creator))]
    public required Guid CreatorId { get; set; }

    public User Creator { get; set; } = null!;

    public bool IsCircuit { get; set; }

    public bool NoMatch { get; set; }

    public bool FreeFeet { get; set; }

    public ICollection<BoulderLog> BoulderLogs { get; set; } = [];
}
