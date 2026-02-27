using System;

namespace Thecell.Bibaboulder.Model.Dto;

public class LineDto
{
    public required Guid Id { get; set; }
    public required string Identifier { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Name { get; set; }
}
