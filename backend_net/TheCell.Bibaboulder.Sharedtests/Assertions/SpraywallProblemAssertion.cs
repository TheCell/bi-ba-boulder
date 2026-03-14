using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SpraywallProblemAssertion
{
    public static void Assert(SpraywallProblem expected, SpraywallProblemDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal((int?)expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.CreatorId, actual.CreatedById);
        Xunit.Assert.Equal(expected.CreatedDate.ToString("u"), actual.CreatedDate);
    }

    public static void Assert(UpdateSpraywallProblemCommand expected, SpraywallProblem actual)
    {
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal((FontGrade?)expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.Version + 1, actual.Version);
    }

    public static void Assert(CreateSpraywallProblemCommand expected, SpraywallProblemDto actual)
    {
        actual.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.Image, actual.Image);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
    }
}
