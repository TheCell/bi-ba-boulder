using System.Linq;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class OutdoorMapping
{
    public static OutdoorAreaDto MapToOutdoorAreaDto(this OutdoorArea outdoorArea)
    {
        return new OutdoorAreaDto
        {
            Id = outdoorArea.Id,
            Name = outdoorArea.Name,
            Description = outdoorArea.Description,
            ImportantInfo = outdoorArea.ImportantInfo,
            PreviewImageUri = outdoorArea.PreviewImageUri,
            Images = outdoorArea.Media.Select(m => m.MapToPublicResourceDto()).ToList(),
            Sectors = outdoorArea.Sectors.Select(s => s.MapToSectorDto()).ToList()
        };
    }

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

    public static PublicResourceDto MapToPublicResourceDto(this PublicResource publicResource)
    {
        return new PublicResourceDto
        {
            Uri = publicResource.Uri,
            ResourceType = publicResource.ResourceType
        };
    }
}
