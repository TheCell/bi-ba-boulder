using System.Linq;
using Thecell.Bibaboulder.Model.Dto.Outdoor;
using Thecell.Bibaboulder.Model.Model.Outdoor;
using TheCell.Bibaboulder.Sharedtests.Extensions;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class BlocAssertion
{
    public static void Assert(Bloc expected, BlocDto actual)
    {
        Xunit.Assert.Equal(expected.Id, actual.Id);
        expected.Id.AssertV7();
        Xunit.Assert.Equal(expected.Name, actual.Name);
        Xunit.Assert.Equal(expected.Coordinates, actual.Coordinates);
        Xunit.Assert.Equal(expected.Description, actual.Description);
        Xunit.Assert.Equal(expected.BlocLowRes, actual.BlocLowRes);
        Xunit.Assert.Equal(expected.BlocMedRes, actual.BlocMedRes);
        Xunit.Assert.Equal(expected.BlocHighRes, actual.BlocHighRes);
        Xunit.Assert.Equal(expected.PreviewImageUri, actual.PreviewImageUri);

        Xunit.Assert.Equal(expected.AdditionalParts.Count, actual.AdditionalParts.Length);
        foreach (var bloc in actual.AdditionalParts)
        {
            var expectedBloc = expected.AdditionalParts.First(b => b.Id == bloc.Id);
            Assert(expectedBloc, bloc);
        }
    }
}
