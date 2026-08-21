using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Dto.Media;

public class PublicResourceDto
{
    public required ResourceType ResourceType { get; set; }
    public required string Uri { get; set; }
}
