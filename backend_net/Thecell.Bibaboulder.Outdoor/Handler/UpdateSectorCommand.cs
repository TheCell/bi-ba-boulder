using System;
using System.Collections.Generic;
using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateSectorCommand : UpdateContentCommand
{
    public bool IsPublic { get; set; }

    public string? Coordinates { get; set; }

    public ICollection<Guid> OutdoorAreaIds { get; set; } = [];
}
