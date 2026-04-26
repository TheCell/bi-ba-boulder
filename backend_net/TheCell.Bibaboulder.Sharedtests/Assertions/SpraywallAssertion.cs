using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Spraywall.Handler;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SpraywallAssertion
{
    public static void Assert(Spraywall expected, SpraywallDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }

    public static void Assert(CreateSpraywallProblemCommand expected, SpraywallProblem actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
    }
}
