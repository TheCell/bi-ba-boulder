using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Common.Queries;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Dto;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Mapping;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class GetSpraywallProblemQueryHandler : IQueryHandler<GetSpraywallProblemQuery, SpraywallProblemDto>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISpraywallImageService _imageService;

    public GetSpraywallProblemQueryHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService,
        ISpraywallImageService imageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _imageService = imageService;
    }

    public async Task<SpraywallProblemDto> HandleAsync(GetSpraywallProblemQuery query)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();
        var isAdmin = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Admin);
        var hasEditorRole = currentUser is not null && currentUser.Roles.Contains(AuthorizationRoles.Editor);

        var problem = await _dbContext.SpraywallProblems
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == query.Id)
            .ThrowIfNullAsync(query.Id);

        var creator = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == problem.CreatorId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync() ?? "Unknown";

        var image = await _imageService.GetImageAsBase64Async(problem.SpraywallId, problem.Id);
        if (image == null)
        {
            throw new NotFoundException($"Image for spraywall problem with id {problem.Id} not found.");
        }

        var spraywallProblemDto = problem.MapToSpraywallProblemDto(creator, image);
        spraywallProblemDto.Metadata.CanEdit = currentUser != null && currentUser.Id == problem.CreatorId && hasEditorRole;
        spraywallProblemDto.Metadata.CanDelete = currentUser != null && ((currentUser.Id == problem.CreatorId && hasEditorRole) || isAdmin);
        return spraywallProblemDto;
    }
}
