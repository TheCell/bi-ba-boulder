using System;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class UpdateBoulderLogCommand
{
    public required Guid Id { get; set; }
    public required long Version { get; set; }
    public required bool IsSent { get; set; }
    public required bool IsProject { get; set; }
    public Rating? Rating { get; set; }
    public FontGrade? FontGrade { get; set; }
}
