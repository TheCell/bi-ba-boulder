using System;
using Thecell.Bibaboulder.Model.Dto.Media;
using Thecell.Bibaboulder.Model.Model.Media;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class UriAliasMapping
{
    public static UriAliasAdministrationDto MapToUriAliasDto(this UriAlias uriAlias)
    {
        return new UriAliasAdministrationDto
        {
            Id = uriAlias.Id,
            Alias = uriAlias.Alias,
            TypeId = (long)uriAlias.Type,
            TargetId = uriAlias.BoulderGymId ?? uriAlias.OutdoorAreaId ?? Guid.Empty,
            TargetName = uriAlias.BoulderGym != null ? uriAlias.BoulderGym.Name : uriAlias.OutdoorArea!.Name,
            Version = uriAlias.Version
        };
    }
}
