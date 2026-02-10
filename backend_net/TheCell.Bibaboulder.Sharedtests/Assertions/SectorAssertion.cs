using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SectorAssertion
{
    public static void Assert(SectorDto actual, Sector expected)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }
}
