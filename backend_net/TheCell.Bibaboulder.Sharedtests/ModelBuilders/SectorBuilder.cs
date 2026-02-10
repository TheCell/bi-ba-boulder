using System;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class SectorBuilder : BuilderBase<Sector>
{
    public SectorBuilder() : base()
    {
        _instance.Id = Guid.NewGuid();
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
}
