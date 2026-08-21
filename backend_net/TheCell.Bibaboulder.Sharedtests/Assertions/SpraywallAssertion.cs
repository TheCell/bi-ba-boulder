using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SpraywallAssertion
{
    public static void Assert(Spraywall expected, SpraywallDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.IsArchived, actual.IsArchived);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.PreviewImageUri, actual.PreviewImageUri);
    }

    public static void Assert(CreateSpraywallProblemCommand expected, SpraywallProblem actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.IsCircuit, actual.IsCircuit);
        Xunit.Assert.Equal(expected.NoMatch, actual.NoMatch);
        Xunit.Assert.Equal(expected.FreeFeet, actual.FreeFeet);
    }
}
