using System.Linq;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class OutdoorAreaAssertion
{
    public static void Assert(OutdoorArea expected, OutdoorAreaDto actual)
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

        Xunit.Assert.Equal(expected.Sectors.Count, actual.Sectors.Count);
        foreach (var sector in actual.Sectors)
        {
            var expectedSector = expected.Sectors.Single(s => s.Id == sector.Id);
            SectorAssertion.Assert(expectedSector, sector);
        }
    }
}
