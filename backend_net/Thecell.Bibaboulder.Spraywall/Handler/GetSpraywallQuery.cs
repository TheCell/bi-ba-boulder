using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Indoor;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class GetSpraywallQuery : IQuery<SpraywallDto>
{
    public required Guid Id { get; set; }
}
