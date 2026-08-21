using System.Linq;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class BoulderGymMapping
{
    public static BoulderGymDto MapToBoulderGymDto(this BoulderGym boulderGym)
    {
        return new BoulderGymDto
        {
            Id = boulderGym.Id,
            Name = boulderGym.Name,
            Description = boulderGym.Description,
            ImportantInfo = boulderGym.ImportantInfo,
            PreviewImageUri = boulderGym.PreviewImageUri,
            Images = boulderGym.Media.Select(m => m.MapToPublicResourceDto()).ToList(),
            Spraywalls = boulderGym.Spraywalls.Select(s => s.MapToSpraywallDto()).ToList()
        };
    }
}
