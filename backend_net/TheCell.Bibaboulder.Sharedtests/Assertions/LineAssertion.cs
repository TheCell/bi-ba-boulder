using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class LineAssertion
{
    public static void Assert(Line expected, LineDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Identifier, actual.Identifier);
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Assert(expected.Data, actual.Data);
    }

    public static void Assert(LineData expected, LineData actual)
    {
        Xunit.Assert.Equal(expected.Positions.Count, actual.Positions.Count);
        for (var i = 0; i < expected.Positions.Count; i++)
        {
            var expectedPosition = expected.Positions[i];
            var actualPosition = actual.Positions[i];
            Xunit.Assert.Equal(expectedPosition[0], actualPosition[0]);
            Xunit.Assert.Equal(expectedPosition[1], actualPosition[1]);
            Xunit.Assert.Equal(expectedPosition[2], actualPosition[2]);
        }
    }
}
