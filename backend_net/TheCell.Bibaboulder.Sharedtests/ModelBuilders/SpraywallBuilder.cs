using System;
using Bogus;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class SpraywallBuilder : BuilderBase<Spraywall>
{
    public SpraywallBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
    }

    public SpraywallBuilder SetName(string value)
    {
        _instance.Name = value;
        return this;
    }

    public SpraywallBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }
}
