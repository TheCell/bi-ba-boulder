using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model.Outdoor;

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

    public string? ImportantInfo { get; set; }

    public bool IsPublic { get; set; }

    public string? Coordinates { get; set; }

    public string? PreviewImageUri { get; set; }

    public ICollection<PublicResource> Media { get; set; } = [];

    public ICollection<Bloc> Blocs { get; set; } = [];

    public ICollection<OutdoorArea> OutdoorAreas { get; set; } = [];
}
