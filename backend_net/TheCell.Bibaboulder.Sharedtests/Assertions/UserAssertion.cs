using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class UserAssertion
{
    public static void Assert(User expected, UserDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Email, actual.Email);
        Xunit.Assert.NotNull(actual.Roles);
        Xunit.Assert.Equal(expected.Roles.Split(','), actual.Roles);
    }
}
