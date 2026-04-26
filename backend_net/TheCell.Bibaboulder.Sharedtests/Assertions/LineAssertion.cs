using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class LineAssertion
{
    public static void Assert(LineDto actual, Line expected)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Identifier, actual.Identifier);
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Color, actual.Color);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }
}
