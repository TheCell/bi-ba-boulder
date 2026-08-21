using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Model.Indoor;

public class BoulderGym : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gym name ex. Bimano
    /// </summary>
    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ImportantInfo { get; set; }

    public string? PreviewImageUri { get; set; }

    public ICollection<PublicResource> Media { get; set; } = [];

    public ICollection<Spraywall> Spraywalls { get; set; } = [];
}
