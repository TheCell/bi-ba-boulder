using System;
using System.Collections.Generic;
using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateOutdoorAreaCommand : UpdateContentCommand
{
    public ICollection<Guid> SectorIds { get; set; } = [];
}
