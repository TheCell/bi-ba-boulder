using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class UpdateSpraywallProblemCommand : UpdateCommand
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Image { get; set; }
    public FontGrade? FontGrade { get; set; }
}
