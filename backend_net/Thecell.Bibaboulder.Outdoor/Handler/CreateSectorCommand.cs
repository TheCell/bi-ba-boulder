using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateSectorCommand : CreateCommand
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? ImportantInfo { get; set; }

    public bool IsPublic { get; set; }

    public string? Coordinates { get; set; }

    public string? PreviewImageUri { get; set; }
}
