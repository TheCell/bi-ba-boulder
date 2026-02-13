using System;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Outdoor.Handler;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SectorAssertion
{
    public static void Assert(SectorDto actual, Sector expected)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }

    public static void Assert(CreateSectorCommand actual, Sector expected)
    {
        Xunit.Assert.NotEqual(Guid.Empty, actual.Id);
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
    }
}
