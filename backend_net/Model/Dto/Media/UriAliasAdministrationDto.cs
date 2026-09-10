using System;

namespace Thecell.Bibaboulder.Model.Dto.Media;

public class UriAliasAdministrationDto
{
    public required Guid Id { get; set; }
    public required string Alias { get; set; }
    public required long TypeId { get; set; }
    public required Guid TargetId { get; set; }
    public required string TargetName { get; set; }
    public required long Version { get; set; }
}
