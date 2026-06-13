using System;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class BoulderLogBuilder : BuilderBase<BoulderLog>
{
    public BoulderLogBuilder() : base()
    {
        _instance.Id = Guid.CreateVersion7();
        _instance.IsSent = false;
        _instance.IsProject = false;
        _instance.UserId = Guid.CreateVersion7();
    }

    public BoulderLogBuilder SetIsSent(bool value)
    {
        _instance.IsSent = value;
        return this;
    }

    public BoulderLogBuilder SetIsProject(bool value)
    {
        _instance.IsProject = value;
        return this;
    }

    public BoulderLogBuilder SetRating(Rating? value)
    {
        _instance.Rating = value;
        return this;
    }

    public BoulderLogBuilder SetFontGrade(FontGrade? value)
    {
        _instance.FontGrade = value;
        return this;
    }

    public BoulderLogBuilder SetUser(User value)
    {
        _instance.User = value;
        _instance.UserId = value.Id;
        return this;
    }

    public BoulderLogBuilder SetSpraywallProblem(SpraywallProblem? value)
    {
        _instance.SpraywallProblem = value;
        _instance.SpraywallProblemId = value?.Id;
        return this;
    }
}
