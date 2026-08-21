using System.Linq;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using Thecell.Bibaboulder.Outdoor.Handler;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class SectorAssertion
{
    public static void Assert(Sector expected, SectorDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.ImportantInfo, actual.ImportantInfo);
        Xunit.Assert.Equal(expected.IsPublic, actual.IsPublic);
        Xunit.Assert.Equal(expected.Coordinates, actual.Coordinates);
        Xunit.Assert.Equal(expected.PreviewImageUri, actual.PreviewImageUri);

        Xunit.Assert.Equal(expected.Media.Count, actual.Images.Count);
        foreach (var image in actual.Images)
        {
            var expectedImage = expected.Media.Single(i => i.Uri == image.Uri && i.ResourceType == image.ResourceType);
            PublicResourceAssertion.Assert(expectedImage, image);
        }
    }

    public static void Assert(CreateSectorCommand expected, Sector actual)
    {
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.ImportantInfo, actual.ImportantInfo);
        Xunit.Assert.Equal(expected.IsPublic, actual.IsPublic);
        Xunit.Assert.Equal(expected.Coordinates, actual.Coordinates);
        Xunit.Assert.Equal(expected.PreviewImageUri, actual.PreviewImageUri);
    }
}
