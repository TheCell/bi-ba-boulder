using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class Bookmark : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    public required bool IsProject { get; set; }

    public required bool IsFavourite { get; set; }

    [ForeignKey(nameof(User))]
    public required Guid UserId { get; set; }

    public User User { get; set; } = null!;

    // TODO: Add relationship to SpraywallProblem - missing in PHP entity
    // public Guid? SpraywallProblemId { get; set; }
    // public SpraywallProblem? SpraywallProblem { get; set; }
}
