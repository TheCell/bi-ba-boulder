using System;
using System.Text.Json.Serialization;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class DeleteSpraywallProblemCommand
{
    [JsonIgnore]
    public Guid Id { get; set; }
}
