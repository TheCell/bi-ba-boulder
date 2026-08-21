using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogBySpraywallQuery : IQuery<BoulderLogDto?>
{
    public required Guid SpraywallProblemId { get; init; }
}
