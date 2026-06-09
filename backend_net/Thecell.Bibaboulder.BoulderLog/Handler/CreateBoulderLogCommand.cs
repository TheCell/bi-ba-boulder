using System;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class CreateBoulderLogCommand
{
    public Guid Id { get; set; }
    public required bool IsSent { get; set; }
    public required bool IsProject { get; set; }
    public Rating? Rating { get; set; }
    public FontGrade? FontGrade { get; set; }
    public Guid? SpraywallProblemId { get; set; }
}
