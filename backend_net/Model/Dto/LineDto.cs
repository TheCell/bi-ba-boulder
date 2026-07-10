using System;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Dto;

public class LineDto
{
    public required Guid Id { get; set; }
    public required string Identifier { get; set; }
    public FontGrade? FontGrade { get; set; }
    public string? Description { get; set; }
    public string? Name { get; set; }
    public LineData Data { get; set; } = new LineData();
}
