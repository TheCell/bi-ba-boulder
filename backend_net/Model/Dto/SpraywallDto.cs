using System;

namespace Thecell.Bibaboulder.Model.Dto;

public class SpraywallDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
