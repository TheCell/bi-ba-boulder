using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SectorAssertion
{
    public static void Assert(Sector expected, SectorDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }

    public static void Assert(CreateSectorCommand expected, Sector actual)
    {
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }
}
