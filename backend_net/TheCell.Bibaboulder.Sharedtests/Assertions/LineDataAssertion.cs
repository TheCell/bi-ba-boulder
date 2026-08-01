using System;
using System.Collections.Generic;
using System.Text;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.Assertions;

public static class LineDataAssertion
{
    public static void Assert(LineData expected, LineData actual)
    {
        Xunit.Assert.Equal(expected.Positions.Count, actual.Positions.Count);
        for (var i = 0; i < expected.Positions.Count; i++)
        {
            var expectedPosition = expected.Positions[i];
            var actualPosition = actual.Positions[i];
            Xunit.Assert.Equal(expectedPosition[0], actualPosition[0]);
            Xunit.Assert.Equal(expectedPosition[1], actualPosition[1]);
            Xunit.Assert.Equal(expectedPosition[2], actualPosition[2]);
        }

        Xunit.Assert.Equal(expected.SceneMarkings.Count, actual.SceneMarkings.Count);
        for (var i = 0; i < expected.SceneMarkings.Count; i++)
        {
            var expectedSceneMarking = expected.SceneMarkings[i];
            var actualSceneMarking = actual.SceneMarkings[i];
            Assert(expectedSceneMarking, actualSceneMarking);
        }
    }

    public static void Assert(SceneMarking expected, SceneMarking actual)
    {
        Xunit.Assert.Equal(expected.Position.Length, actual.Position.Length);
        Xunit.Assert.Equal(expected.Scale.Length, actual.Scale.Length);
        Xunit.Assert.Equal(expected.Quaternion.Length, actual.Quaternion.Length);

        Xunit.Assert.Equal(expected.Type, actual.Type);
        Xunit.Assert.Equal(expected.Form, actual.Form);
        Xunit.Assert.Equal(expected.Position[0], actual.Position[0]);
        Xunit.Assert.Equal(expected.Position[1], actual.Position[1]);
        Xunit.Assert.Equal(expected.Position[2], actual.Position[2]);
        Xunit.Assert.Equal(expected.Scale[0], actual.Scale[0]);
        Xunit.Assert.Equal(expected.Scale[1], actual.Scale[1]);
        Xunit.Assert.Equal(expected.Scale[2], actual.Scale[2]);
        Xunit.Assert.Equal(expected.Quaternion[0], actual.Quaternion[0]);
        Xunit.Assert.Equal(expected.Quaternion[1], actual.Quaternion[1]);
        Xunit.Assert.Equal(expected.Quaternion[2], actual.Quaternion[2]);
        Xunit.Assert.Equal(expected.Quaternion[3], actual.Quaternion[3]);
    }
}
