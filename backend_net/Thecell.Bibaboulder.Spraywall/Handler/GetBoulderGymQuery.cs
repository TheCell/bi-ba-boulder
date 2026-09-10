using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class GetBoulderGymQuery : IQuery<BoulderGymDto>
{
    public required Guid Id { get; init; }
}
