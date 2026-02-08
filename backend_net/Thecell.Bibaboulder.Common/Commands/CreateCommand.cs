using System;
using System.Text.Json.Serialization;

namespace Thecell.Bibaboulder.Common.Commands;

public abstract class CreateCommand
{
    [JsonIgnore]
    public Guid Id { get; set; }
}
