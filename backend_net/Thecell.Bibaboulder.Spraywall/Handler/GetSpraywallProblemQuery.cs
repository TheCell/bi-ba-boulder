using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class GetSpraywallProblemQuery : IQuery<SpraywallProblemDto>
{
    public required Guid Id { get; init; }
}
