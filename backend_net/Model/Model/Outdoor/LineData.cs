using System.Collections.Generic;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Model.Outdoor;

public class LineData
{
    public List<double[]> Positions { get; set; } = [];
    public string? SomeMoreStuff { get; set; } = string.Empty;
    public List<SceneMarking> SceneMarkings { get; set; } = [];
}

public class SceneMarking
{
    public SceneMarkingType Type { get; set; }
    public SceneMarkingForm Form { get; set; }
    public double[] Position { get; set; } = []; // X, Y, Z
    public double[] Scale { get; set; } = []; // X, Y, Z
    public double[] Quaternion { get; set; } = []; // X, Y, Z, W
}
