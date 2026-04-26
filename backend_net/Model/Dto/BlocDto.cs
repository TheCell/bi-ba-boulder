using System;

namespace Thecell.Bibaboulder.Model.Dto;

public class BlocDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? BlocLowRes { get; set; }
    public string? BlocMedRes { get; set; }
    public string? BlocHighRes { get; set; }
}
