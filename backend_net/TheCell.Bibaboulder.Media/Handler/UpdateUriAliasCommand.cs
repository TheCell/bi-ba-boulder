using Thecell.Bibaboulder.Common.Commands;

namespace TheCell.Bibaboulder.Media.Handler;

public class UpdateUriAliasCommand : UpdateCommand
{
    public required string Alias { get; set; }
    public required long TypeId { get; set; }
    public required Guid TargetId { get; set; }
}
