namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SearchProblemsRequest
{
    public int? GradeMin { get; set; }
    public int? GradeMax { get; set; }
    public string? Name { get; set; }
    public string? Creator { get; set; }
    public string? DateOrder { get; set; }
    public int? Page { get; set; }
}
