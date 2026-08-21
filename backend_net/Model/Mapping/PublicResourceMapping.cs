using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class PublicResourceMapping
{
    public static PublicResourceDto MapToPublicResourceDto(this PublicResource publicResource)
    {
        return new PublicResourceDto
        {
            Uri = publicResource.Uri,
            ResourceType = publicResource.ResourceType
        };
    }
}
