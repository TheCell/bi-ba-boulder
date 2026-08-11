using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model.Outdoor;
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
        Xunit.Assert.Equal(expected.Version, actual.Version);
        LineDataAssertion.Assert(expected.Data, actual.Data);
    }
}
