using Thecell.Bibaboulder.BoulderLog.Handler;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class BoulderLogAssertion
{
    public static void Assert(BoulderLog expected, BoulderLogDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        Xunit.Assert.Equal(expected.IsSent, actual.IsSent);
        Xunit.Assert.Equal(expected.IsProject, actual.IsProject);
        Xunit.Assert.Equal(expected.Rating, actual.Rating);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.UserId, actual.UserId);
        Xunit.Assert.Equal(expected.SpraywallProblemId, actual.SpraywallProblemId);
    }

    public static void Assert(CreateBoulderLogCommand expected, BoulderLogDto actual)
    {
        Xunit.Assert.Equal(expected.IsSent, actual.IsSent);
        Xunit.Assert.Equal(expected.IsProject, actual.IsProject);
        Xunit.Assert.Equal(expected.Rating, actual.Rating);
        Xunit.Assert.Equal(expected.FontGrade, actual.FontGrade);
        Xunit.Assert.Equal(expected.SpraywallProblemId, actual.SpraywallProblemId);
    }
}
