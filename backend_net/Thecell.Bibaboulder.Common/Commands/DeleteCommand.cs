using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Thecell.Bibaboulder.Common.Commands;

public abstract class DeleteCommand
{
    [JsonIgnore]
    public Guid Id { get; set; }

    [ConcurrencyCheck]
    public long Version { get; set; }
}
