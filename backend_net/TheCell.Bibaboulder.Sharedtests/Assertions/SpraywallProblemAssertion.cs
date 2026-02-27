using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SpraywallProblemAssertion
{
    public static void Assert(SpraywallProblemDto actual, SpraywallProblem expected, string expectedCreatorName)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.CreatedUserId, actual.CreatedById);
        Xunit.Assert.Equal(expectedCreatorName, actual.CreatedByName);
    }
}
