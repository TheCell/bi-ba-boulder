using System;
using System.Collections.Generic;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogsQuery : IQuery<ICollection<BoulderLogDto>>
{
    public required Guid UserId { get; init; }
}
