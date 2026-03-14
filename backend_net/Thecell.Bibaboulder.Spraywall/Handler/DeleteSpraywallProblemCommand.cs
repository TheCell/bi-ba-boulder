using System;
using System.Text.Json.Serialization;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class DeleteSpraywallProblemCommand
{
    [JsonIgnore]
    public Guid Id { get; set; }
}
