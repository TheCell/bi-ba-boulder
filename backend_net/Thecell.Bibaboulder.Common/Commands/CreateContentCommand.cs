using System.Collections.Generic;

namespace Thecell.Bibaboulder.Common.Commands;

public abstract class CreateContentCommand : CreateCommand
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ImportantInfo { get; set; }

    public string? PreviewImageUri { get; set; }

    public ICollection<string> ImageUris { get; set; } = [];
}