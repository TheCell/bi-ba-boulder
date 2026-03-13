using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class SearchSpraywallProblemsQuery : IQuery<SpraywallProblemListDto>
{
    public required Guid SpraywallId { get; init; }
    public int? GradeMin { get; init; }
    public int? GradeMax { get; init; }
    public string? Name { get; init; }
    public string? Creator { get; init; }
    public string DateOrder { get; init; } = "desc";
    public int Page { get; init; } = 1;
}
