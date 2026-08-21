using System.Linq;
using Thecell.Bibaboulder.Model.Dto.Indoor;
using Thecell.Bibaboulder.Model.Model.Indoor;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class BoulderGymAssertion
{
    public static void Assert(BoulderGym expected, BoulderGymDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.ImportantInfo, actual.ImportantInfo);
        Xunit.Assert.Equal(expected.PreviewImageUri, actual.PreviewImageUri);

        Xunit.Assert.Equal(expected.Media.Count, actual.Images.Count);
        foreach (var image in actual.Images)
        {
            var expectedImage = expected.Media.Single(i => i.Uri == image.Uri && i.ResourceType == image.ResourceType);
            PublicResourceAssertion.Assert(expectedImage, image);
        }

        Xunit.Assert.Equal(expected.Spraywalls.Count, actual.Spraywalls.Count);

        var orderedSpraywalls = actual.Spraywalls.OrderBy(s => s.IsArchived).ToList();
        Xunit.Assert.Equal(orderedSpraywalls, actual.Spraywalls);

        foreach (var spraywall in actual.Spraywalls)
        {
            var expectedSpraywall = expected.Spraywalls.Single(s => s.Id == spraywall.Id);
            SpraywallAssertion.Assert(expectedSpraywall, spraywall);
        }
    }
}
