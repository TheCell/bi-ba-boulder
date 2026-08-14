using System;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteUserSectorAccessCommand
{
    public required Guid UserId { get; set; }
    public required Guid SectorId { get; set; }
    public required long Version { get; set; }
}