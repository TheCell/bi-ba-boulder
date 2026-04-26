using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateSectorCommand : CreateCommand
{
    public required string Name { get; set; }

    public string? Description { get; set; }
}
