using System;
using System.Collections.Generic;

namespace Thecell.Bibaboulder.Model.Dto;

public class SectorDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImportantInfo { get; set; }
    public bool IsPublic { get; set; }
    public string? Coordinates { get; set; }
    public string? PreviewImageUri { get; set; }
    public ICollection<PublicResourceDto> Images { get; set; } = [];
    public ICollection<OutdoorAreaDto> OutdoorAreas { get; set; } = [];
}
