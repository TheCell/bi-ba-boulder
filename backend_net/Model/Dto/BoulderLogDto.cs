using System;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Dto;

public class BoulderLogDto
{
    public required Guid Id { get; set; }
    public required long Version { get; set; }
    public required bool IsSent { get; set; }
    public required bool IsProject { get; set; }
    public Rating? Rating { get; set; }
    public FontGrade? FontGrade { get; set; }
    public required Guid UserId { get; set; }
    public Guid? SpraywallProblemId { get; set; }
}
