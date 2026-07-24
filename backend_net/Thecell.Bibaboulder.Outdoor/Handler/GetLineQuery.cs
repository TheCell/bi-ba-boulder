using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetLineQuery : IQuery<LineDto>
{
    public required Guid Id { get; init; }
}
