using System;
using Bogus;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class SpraywallProblemBuilder : BuilderBase<SpraywallProblem>
{
    public SpraywallProblemBuilder(User creator, Spraywall spraywall) : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
        SetCreator(creator);
        SetSpraywall(spraywall);
    }

    public SpraywallProblemBuilder SetName(string value)
    {
        _instance.Name = value;
        return this;
    }

    public SpraywallProblemBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }

    public SpraywallProblemBuilder SetFontGrade(FontGrade? value)
    {
        _instance.FontGrade = value;
        return this;
    }

    public SpraywallProblemBuilder SetSpraywall(Spraywall value)
    {
        _instance.Spraywall = value;
        _instance.SpraywallId = value.Id;
        return this;
    }

    public SpraywallProblemBuilder SetCreator(User value)
    {
        _instance.Creator = value;
        _instance.CreatorId = value.Id;
        return this;
    }

    public SpraywallProblemBuilder SetIsCircuit(bool value)
    {
        _instance.IsCircuit = value;
        return this;
    }

    public SpraywallProblemBuilder SetNoMatch(bool value)
    {
        _instance.NoMatch = value;
        return this;
    }

    public SpraywallProblemBuilder SetFreeFeet(bool value)
    {
        _instance.FreeFeet = value;
        return this;
    }
}
