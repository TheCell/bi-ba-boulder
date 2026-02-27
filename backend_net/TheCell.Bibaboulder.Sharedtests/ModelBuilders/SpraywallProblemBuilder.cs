using System;
using Bogus;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class SpraywallProblemBuilder : BuilderBase<SpraywallProblem>
{
    public SpraywallProblemBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
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

    public SpraywallProblemBuilder SetSpraywallId(Guid value)
    {
        _instance.SpraywallId = value;
        return this;
    }

    public SpraywallProblemBuilder SetCreatedUserId(Guid value)
    {
        _instance.CreatedUserId = value;
        return this;
    }
}
