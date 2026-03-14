using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public class DeleteSpraywallProblemCommandHandler : ICommandHandler<DeleteSpraywallProblemCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISpraywallImageService _imageService;

    public DeleteSpraywallProblemCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService,
        ISpraywallImageService imageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _imageService = imageService;
    }

    public async Task HandleAsync(DeleteSpraywallProblemCommand command)
    {
        var problem = await _dbContext.SpraywallProblems
            .SingleOrDefaultAsync(p => p.Id == command.Id);
        NotFoundException.ThrowIfNull(problem, nameof(Model.Model.SpraywallProblem), command.Id);

        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();
        var isCreator = currentUser.Id == problem.CreatorId;
        var isAdmin = currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!isCreator && !isAdmin)
        {
            throw new AccessDeniedException("Only the creator or an admin can delete this problem");
        }

        if (!isAdmin && !currentUser.IsInRole(UserRole.Editor))
        {
            throw new AccessDeniedException("Only users with editor role or higher can delete problems");
        }

        var spraywallId = problem.SpraywallId;
        await _dbContext.RemoveEntityAsync(problem, problem.Version);
        await _imageService.DeleteImageAsync(spraywallId, command.Id);
    }
}
