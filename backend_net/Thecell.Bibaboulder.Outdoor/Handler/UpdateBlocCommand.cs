using System;
using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateBlocCommand : UpdateCommand
{
    public required Guid SectorId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public string? Coordinates { get; set; }

    public string? BlocLowRes { get; set; }

    public string? BlocMedRes { get; set; }

    public string? BlocHighRes { get; set; }

    public string? PreviewImageUri { get; set; }
}
