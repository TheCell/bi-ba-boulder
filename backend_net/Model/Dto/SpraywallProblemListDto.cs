using System.Collections.Generic;

namespace Thecell.Bibaboulder.Model.Dto;

public class SpraywallProblemListDto
{
    public required int TotalCount { get; set; }
    public required int CurrentPage { get; set; }
    public required ICollection<SpraywallProblemDto> Problems { get; set; }
}
