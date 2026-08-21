using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class LineMapping
{
    public static LineDto MapToLineDto(this Line line)
    {
        return new LineDto
        {
            Id = line.Id,
            Identifier = line.Identifier,
            Description = line.Description,
            Name = line.Name,
            Data = line.Data,
            FontGrade = line.FontGrade,
            Version = line.Version,
            Metadata = new LineMetadataDto
            {
                CanEdit = false,
                CanDelete = false
            }
        };
    }
}
