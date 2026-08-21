using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class GetUserByIdQuery : IQuery<UserDto>
{
    public required Guid Id { get; init; }
}
