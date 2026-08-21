using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Outdoor;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetOutdoorAreaQuery : IQuery<OutdoorAreaDto>
{
    public required Guid Id { get; init; }
}
