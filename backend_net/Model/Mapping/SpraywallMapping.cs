using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class SpraywallMapping
{
    public static SpraywallDto MapToSpraywallDto(this Spraywall spraywall)
    {
        return new SpraywallDto
        {
            Id = spraywall.Id,
            Name = spraywall.Name,
            IsArchived = spraywall.IsArchived,
            Description = spraywall.Description,
            PreviewImageUri = spraywall.PreviewImageUri
        };
    }
}
