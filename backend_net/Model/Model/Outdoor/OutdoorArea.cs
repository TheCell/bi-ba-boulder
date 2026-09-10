using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Thecell.Bibaboulder.Model.Basics;
using Thecell.Bibaboulder.Model.Model.Media;

namespace Thecell.Bibaboulder.Model.Model.Outdoor;

public class OutdoorArea : VersionedEntity, IContentEntity
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

    public ICollection<UriAlias> UriAliases { get; set; } = [];
}
