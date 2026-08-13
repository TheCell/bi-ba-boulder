using System;
using System.Collections.Generic;
using Bogus;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class SectorBuilder : BuilderBase<Sector>
{
    public SectorBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
    }

    public SectorBuilder SetName(string value)
    {
        _instance.Name = value;
        return this;
    }

    public SectorBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }

    public SectorBuilder SetCoordinates(string? value)
    {
        _instance.Coordinates = value;
        return this;
    }

    public SectorBuilder SetImportantInfo(string? value)
    {
        _instance.ImportantInfo = value;
        return this;
    }

    public SectorBuilder SetIsPublic(bool value)
    {
        _instance.IsPublic = value;
        return this;
    }

    public SectorBuilder SetPreviewImageUri(string? value)
    {
        _instance.PreviewImageUri = value;
        return this;
    }

    public SectorBuilder SetImages(ICollection<PublicResource> value)
    {
        _instance.Media = value;
        return this;
    }
}
