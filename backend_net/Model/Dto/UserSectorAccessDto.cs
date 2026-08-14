using System;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Model.Dto;

public class UserSectorAccessDto
{
    public required Guid UserId { get; set; }
    public required Guid SectorId { get; set; }
    public required AccessSourceType AccessSourceType { get; set; }
    public DateTime? ValidUntil { get; set; }
    public required long Version { get; set; }
}