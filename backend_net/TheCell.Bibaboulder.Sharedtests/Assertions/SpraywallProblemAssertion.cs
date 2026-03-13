using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SpraywallProblemAssertion
{
    public static void Assert(SpraywallProblem expected, SpraywallProblemDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal((int?)expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.CreatedUserId, actual.CreatedById);
        Xunit.Assert.Equal(expected.CreatedDate.ToString("o"), actual.CreatedDate);
    }
}
