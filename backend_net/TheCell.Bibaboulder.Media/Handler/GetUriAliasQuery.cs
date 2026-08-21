using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Enums;

namespace TheCell.Bibaboulder.Media.Handler;

public class GetUriAliasQuery : IQuery<UriAliasDto>
{
    public required string Alias { get; set; }

    public required UriType Type { get; set; }
}
