using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogQuery : IQuery<BoulderLogDto>
{
    public required Guid Id { get; init; }
}
