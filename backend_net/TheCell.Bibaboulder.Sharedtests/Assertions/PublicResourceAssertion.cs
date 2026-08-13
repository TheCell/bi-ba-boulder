using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class PublicResourceAssertion
{
    public static void Assert(PublicResource expected, PublicResourceDto actual)
    {
        Xunit.Assert.Equal(expected.ResourceType, actual.ResourceType);
        Xunit.Assert.Equal(expected.Uri, actual.Uri);
    }
}
