using System;

namespace Thecell.Bibaboulder.Model.Dto.Indoor;

public class SpraywallDto
{
    public required Guid Id { get; set; }
    public bool IsArchived { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? PreviewImageUri { get; set; }
}
