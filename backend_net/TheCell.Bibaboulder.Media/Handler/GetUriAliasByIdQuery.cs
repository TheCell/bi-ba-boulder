using System;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Media;

namespace TheCell.Bibaboulder.Media.Handler;

public class GetUriAliasByIdQuery : IQuery<UriAliasAdministrationDto>
{
    public required Guid Id { get; init; }
}