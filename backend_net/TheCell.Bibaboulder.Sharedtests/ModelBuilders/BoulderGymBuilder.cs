using System;
using System.Collections.Generic;
using Bogus;
using Thecell.Bibaboulder.Model.Model.Indoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class BoulderGymBuilder : BuilderBase<BoulderGym>
{
    public BoulderGymBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
    }

    public BoulderGymBuilder SetName(string value)
    {
        _instance.Name = value;
        return this;
    }

    public BoulderGymBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }

    public BoulderGymBuilder SetImportantInfo(string? value)
    {
        _instance.ImportantInfo = value;
        return this;
    }

    public BoulderGymBuilder SetPreviewImageUri(string? value)
    {
        _instance.PreviewImageUri = value;
        return this;
    }

    public BoulderGymBuilder SetImages(ICollection<PublicResource> value)
    {
        _instance.Media = value;
        return this;
    }

    public BoulderGymBuilder SetSpraywalls(ICollection<Spraywall> value)
    {
        _instance.Spraywalls = value;
        return this;
    }
}
