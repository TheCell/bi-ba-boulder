using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class SpraywallMapping
{
    public static SpraywallDto MapToSpraywallDto(this Spraywall spraywall)
    {
        return new SpraywallDto
        {
            Id = spraywall.Id,
            Name = spraywall.Name,
            Description = spraywall.Description
        };
    }
}
