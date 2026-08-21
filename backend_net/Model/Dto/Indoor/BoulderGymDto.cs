using System;
using System.Collections.Generic;
using Thecell.Bibaboulder.Model.Dto.Media;

namespace Thecell.Bibaboulder.Model.Dto.Indoor;

public class BoulderGymDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImportantInfo { get; set; }
    public string? PreviewImageUri { get; set; }
    public ICollection<PublicResourceDto> Images { get; set; } = [];
    public ICollection<SpraywallDto> Spraywalls { get; set; } = [];
}
