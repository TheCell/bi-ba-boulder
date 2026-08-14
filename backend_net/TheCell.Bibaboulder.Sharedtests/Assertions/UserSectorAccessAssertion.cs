using Thecell.Bibaboulder.Model.Model.Access;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class UserSectorAccessAssertion
{
    public static void Assert(UserSectorAccess expected, UserSectorAccess actual)
    {
        Xunit.Assert.Equal(expected.UserId, actual.UserId);
        Xunit.Assert.Equal(expected.SectorId, actual.SectorId);
        Xunit.Assert.Equal(expected.AccessSourceType, actual.AccessSourceType);
        Xunit.Assert.Equal(expected.ValidUntil, actual.ValidUntil);
        Xunit.Assert.Equal(expected.Version, actual.Version);
    }
}
