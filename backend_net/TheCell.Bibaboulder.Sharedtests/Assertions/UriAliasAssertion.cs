using System;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Model.Media;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class UriAliasAssertion
{
    public static void Assert(UriAlias expected, UriAliasAdministrationDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Alias, actual.Alias);
        Xunit.Assert.Equal((long)expected.Type, actual.TypeId);
        Xunit.Assert.Equal(expected.BoulderGymId ?? expected.OutdoorAreaId ?? Guid.Empty, actual.TargetId);
        Xunit.Assert.Equal(expected.BoulderGym != null ? expected.BoulderGym.Name : expected.OutdoorArea!.Name, actual.TargetName);
        Xunit.Assert.Equal(expected.Version, actual.Version);
    }
}
