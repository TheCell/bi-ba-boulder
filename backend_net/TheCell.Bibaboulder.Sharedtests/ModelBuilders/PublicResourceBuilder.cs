using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class PublicResourceBuilder : BuilderBase<PublicResource>
{
    public PublicResourceBuilder() : base()
    {
    }

    public PublicResourceBuilder SetUri(string value)
    {
        _instance.Uri = value;
        return this;
    }

    public PublicResourceBuilder SetResourceType(ResourceType value)
    {
        _instance.ResourceType = value;
        return this;
    }
}
