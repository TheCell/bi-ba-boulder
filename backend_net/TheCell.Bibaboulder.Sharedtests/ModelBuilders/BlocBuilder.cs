using System;
using Bogus;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class BlocBuilder : BuilderBase<Bloc>
{
    public BlocBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
    }

    public BlocBuilder SetName(string value)
    {
        _instance.Name = value;
        return this;
    }

    public BlocBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }

    public BlocBuilder SetSectorId(Guid value)
    {
        _instance.SectorId = value;
        return this;
    }

    public BlocBuilder SetBlocLowRes(string? value)
    {
        _instance.BlocLowRes = value;
        return this;
    }

    public BlocBuilder SetBlocMedRes(string? value)
    {
        _instance.BlocMedRes = value;
        return this;
    }

    public BlocBuilder SetBlocHighRes(string? value)
    {
        _instance.BlocHighRes = value;
        return this;
    }
}
