using System;
using Bogus;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class LineBuilder : BuilderBase<Line>
{
    public LineBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.Name = bogus.Lorem.Slug();
    }

    public LineBuilder SetIdentifier(string value)
    {
        _instance.Identifier = value;
        return this;
    }

    public LineBuilder SetName(string? value)
    {
        _instance.Name = value;
        return this;
    }

    public LineBuilder SetFontGrade(FontGrade? value)
    {
        _instance.FontGrade = value;
        return this;
    }

    public LineBuilder SetData(LineData value)
    {
        _instance.Data = value;
        return this;
    }

    public LineBuilder SetDescription(string? value)
    {
        _instance.Description = value;
        return this;
    }

    public LineBuilder SetBlocId(Guid value)
    {
        _instance.BlocId = value;
        return this;
    }

    public LineBuilder SetCreator(User value)
    {
        _instance.CreatedUserId = value.Id;
        return this;
    }
}
