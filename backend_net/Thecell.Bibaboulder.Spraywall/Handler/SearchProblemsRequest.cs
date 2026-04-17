using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SearchProblemsRequest
{
    public FontGrade? GradeMin { get; set; }
    public FontGrade? GradeMax { get; set; }
    public string? Name { get; set; }
    public string? Creator { get; set; }
    public string? DateOrder { get; set; }
    public int? Page { get; set; }
}
