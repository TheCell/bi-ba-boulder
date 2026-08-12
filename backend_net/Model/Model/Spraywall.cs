using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class Spraywall : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(512)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<SpraywallProblem> SpraywallProblems { get; set; } = [];
}
