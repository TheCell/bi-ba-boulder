using System.Linq;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
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
}
