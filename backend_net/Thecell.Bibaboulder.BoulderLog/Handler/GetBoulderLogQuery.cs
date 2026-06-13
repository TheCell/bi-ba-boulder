using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class GetBoulderLogQuery : IQuery<BoulderLogDto?>
{
    public required Guid Id { get; init; }
}
