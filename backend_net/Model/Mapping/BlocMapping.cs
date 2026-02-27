using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Model.Mapping;

public static class BlocMapping
{
    public static BlocDto MapToBlocDto(this Bloc bloc)
    {
        return new BlocDto
        {
            Id = bloc.Id,
            Name = bloc.Name,
            Description = bloc.Description,
            BlocLowRes = bloc.BlocLowRes,
            BlocMedRes = bloc.BlocMedRes,
            BlocHighRes = bloc.BlocHighRes
        };
    }
}
