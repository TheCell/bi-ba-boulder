using System;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateUserSectorAccessCommand
{
    public required Guid UserId { get; set; }
    public required Guid SectorId { get; set; }
    public required AccessSourceType AccessSourceType { get; set; }
    public DateTime? ValidUntil { get; set; }
}