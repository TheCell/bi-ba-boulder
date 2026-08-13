using System;
using System.Collections.Generic;

namespace Thecell.Bibaboulder.Model.Dto;

public class OutdoorAreaDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImportantInfo { get; set; }
    public string? PreviewImageUri { get; set; }
    public ICollection<PublicResourceDto> Images { get; set; } = [];
    public ICollection<SectorDto> Sectors { get; set; } = [];
}
