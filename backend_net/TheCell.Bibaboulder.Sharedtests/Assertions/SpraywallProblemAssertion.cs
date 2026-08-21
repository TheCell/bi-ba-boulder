using Thecell.Bibaboulder.Indoor.Handler;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;
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
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.CreatorId, actual.CreatedById);
        Xunit.Assert.Equal(expected.CreatedDate.ToString("o", System.Globalization.CultureInfo.InvariantCulture), actual.CreatedDate);
        Xunit.Assert.Equal(expected.IsCircuit, actual.IsCircuit);
        Xunit.Assert.Equal(expected.NoMatch, actual.NoMatch);
        Xunit.Assert.Equal(expected.FreeFeet, actual.FreeFeet);
        Xunit.Assert.Equal(expected.IsWip, actual.IsWip);
    }

    public static void Assert(UpdateSpraywallProblemCommand expected, SpraywallProblem actual)
    {
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.Version + 1, actual.Version);
        Xunit.Assert.Equal(expected.IsCircuit, actual.IsCircuit);
        Xunit.Assert.Equal(expected.NoMatch, actual.NoMatch);
        Xunit.Assert.Equal(expected.FreeFeet, actual.FreeFeet);
        Xunit.Assert.Equal(expected.IsWip, actual.IsWip);
    }

    public static void Assert(CreateSpraywallProblemCommand expected, SpraywallProblemDto actual)
    {
        actual.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.Image, actual.Image);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.IsCircuit, actual.IsCircuit);
        Xunit.Assert.Equal(expected.NoMatch, actual.NoMatch);
        Xunit.Assert.Equal(expected.FreeFeet, actual.FreeFeet);
        Xunit.Assert.Equal(expected.IsWip, actual.IsWip);
    }
}
