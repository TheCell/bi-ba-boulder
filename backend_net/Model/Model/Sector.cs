using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model;

public class Sector : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Sector name ex. The Shield
    /// </summary>
    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public ICollection<Bloc> Blocs { get; set; } = [];
}
