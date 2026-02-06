using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Model;

public class BoulderLog : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    public required bool IsSent { get; set; }

    public required bool IsProject { get; set; }

    public Rating? Rating { get; set; }

    public FontGrade? FontGrade { get; set; }

    [ForeignKey(nameof(User))]
    public required Guid UserId { get; set; }

    public User User { get; set; } = null!;

    [ForeignKey(nameof(SpraywallProblem))]
    public Guid? SpraywallProblemId { get; set; }

    public SpraywallProblem? SpraywallProblem { get; set; }

    public ICollection<LogEntry> LogEntries { get; set; } = [];
}
