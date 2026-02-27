using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class BlocAssertion
{
    public static void Assert(BlocDto actual, Bloc expected)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.BlocLowRes, actual.BlocLowRes);
        Xunit.Assert.Equal(expected.BlocMedRes, actual.BlocMedRes);
        Xunit.Assert.Equal(expected.BlocHighRes, actual.BlocHighRes);
    }
}
