using System;

namespace TheCell.Bibaboulder.Sharedtests.Extensions;

public static class GuidExtension
{
    public static void AssertV7(this Guid id)
    {
        var guidString = id.ToString();
        var versionChar = guidString[14]; // Version is at position 14 in format: xxxxxxxx-xxxx-Vxxx-xxxx-xxxxxxxxxxxx
        Xunit.Assert.Equal('7', versionChar);
    }
}
