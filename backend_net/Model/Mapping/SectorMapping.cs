using System.Linq;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class SectorMapping
{
    public static SectorDto MapToSectorDto(this Sector sector)
    {
        return new SectorDto
        {
            Id = sector.Id,
            Name = sector.Name,
            Description = sector.Description,
            ImportantInfo = sector.ImportantInfo,
            IsPublic = sector.IsPublic,
            Coordinates = sector.Coordinates,
            PreviewImageUri = sector.PreviewImageUri,
            Images = sector.Media.Select(m => m.MapToPublicResourceDto()).ToList()
        };
    }
}
