using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Enums;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SearchSpraywallProblemsQuery : IQuery<SpraywallProblemListDto>
{
    public required Guid SpraywallId { get; init; }
    public FontGrade? GradeMin { get; init; }
    public FontGrade? GradeMax { get; init; }
    public string? Name { get; init; }
    public string? Creator { get; init; }
    public string DateOrder { get; init; } = "desc";
    public int Page { get; init; } = 1;
}
