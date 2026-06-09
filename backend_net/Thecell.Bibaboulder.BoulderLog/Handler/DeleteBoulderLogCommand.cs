using System;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class DeleteBoulderLogCommand
{
    public required Guid Id { get; set; }
    public required long Version { get; set; }
}
