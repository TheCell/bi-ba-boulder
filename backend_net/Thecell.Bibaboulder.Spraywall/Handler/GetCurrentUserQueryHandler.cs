using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> HandleAsync(GetCurrentUserQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        return currentUser.MapToUserDto();
    }
}
