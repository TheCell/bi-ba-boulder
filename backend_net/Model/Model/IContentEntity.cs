using System.Collections.Generic;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Model;

public interface IContentEntity
{
    string Name { get; set; }

    string? Description { get; set; }

    string? ImportantInfo { get; set; }

    string? PreviewImageUri { get; set; }

    ICollection<PublicResource> Media { get; set; }
}