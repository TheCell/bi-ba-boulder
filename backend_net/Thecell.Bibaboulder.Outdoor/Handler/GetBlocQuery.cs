using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetBlocQuery : IQuery<BlocDto>
{
    public required Guid Id { get; init; }
}
