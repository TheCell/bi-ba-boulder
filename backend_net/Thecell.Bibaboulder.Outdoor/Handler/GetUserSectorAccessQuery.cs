using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class GetUserSectorAccessQuery : IQuery<UserSectorAccessDto>
{
    public required Guid UserId { get; init; }
    public required Guid SectorId { get; init; }
}