using System;
using System.Collections.Generic;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetBlocsBySectorIdQuery : IQuery<ICollection<BlocDto>>
{
    public required Guid SectorId { get; init; }
}
