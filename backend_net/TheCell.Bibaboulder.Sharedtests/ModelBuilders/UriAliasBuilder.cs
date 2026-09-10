using System;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Indoor;
using Thecell.Bibaboulder.Model.Model.Media;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class UriAliasBuilder : BuilderBase<UriAlias>
{
    public UriAliasBuilder(string alias) : base()
    {
        _instance.Id = Guid.CreateVersion7();
        SetAlias(alias);
    }

    public UriAliasBuilder SetAlias(string value)
    {
        _instance.Alias = value;
        return this;
    }

    public UriAliasBuilder SetType(UriType value)
    {
        _instance.Type = value;
        return this;
    }

    public UriAliasBuilder SetBoulderGym(BoulderGym value)
    {
        _instance.BoulderGym = value;
        _instance.BoulderGymId = value.Id;
        return this;
    }

    public UriAliasBuilder SetOutdoorArea(OutdoorArea value)
    {
        _instance.OutdoorArea = value;
        _instance.OutdoorAreaId = value.Id;
        return this;
    }

}
