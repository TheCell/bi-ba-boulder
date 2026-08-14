using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;

namespace Thecell.Bibaboulder.Model.Model.Outdoor;

public class OutdoorArea : VersionedEntity
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Area name ex. Lindental
    /// </summary>
    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ImportantInfo { get; set; }

    public string? PreviewImageUri { get; set; }

    public ICollection<PublicResource> Media { get; set; } = [];

    public ICollection<Sector> Sectors { get; set; } = [];
}
