using System;
using System.Collections.Generic;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetLinesByBlocIdQuery : IQuery<ICollection<LineDto>>
{
    public required Guid BlocId { get; init; }
}
