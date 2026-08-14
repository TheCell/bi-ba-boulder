using System;
using System.Collections.Generic;
using Bogus;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class OutdoorAreaBuilder : BuilderBase<OutdoorArea>
{
    public OutdoorAreaBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
    }

    public OutdoorAreaBuilder SetName(string value)
    {
        _instance.Name = value;
        return this;
    }

    public OutdoorAreaBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }

    public OutdoorAreaBuilder SetImportantInfo(string? value)
    {
        _instance.ImportantInfo = value;
        return this;
    }

    public OutdoorAreaBuilder SetPreviewImageUri(string? value)
    {
        _instance.PreviewImageUri = value;
        return this;
    }

    public OutdoorAreaBuilder SetImages(ICollection<PublicResource> value)
    {
        _instance.Media = value;
        return this;
    }

    public OutdoorAreaBuilder SetSectors(ICollection<Sector> value)
    {
        _instance.Sectors = value;
        return this;
    }
}
