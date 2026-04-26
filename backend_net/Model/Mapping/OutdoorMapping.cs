using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class OutdoorMapping
{
    public static SectorDto MapToSectorDto(this Sector sector)
    {
        return new SectorDto
        {
            Id = sector.Id,
            Name = sector.Name,
            Description = sector.Description
        };
    }
}
