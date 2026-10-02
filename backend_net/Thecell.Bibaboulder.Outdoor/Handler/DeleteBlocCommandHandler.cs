using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteBlocCommandHandler : ICommandHandler<DeleteBlocCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBlocCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteBlocCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var bloc = await _dbContext.Blocs
            .Include(item => item.AdditionalParts)
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        if (!currentUser.IsInRole(UserRole.Admin) && currentUser.IsInRole(UserRole.ContentAdmin) && currentUser.Id != bloc.CreatedUserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this bloc.");
        }

        await _dbContext.RemoveEntityAndSaveChangesAsync(bloc, command.Version);
    }
}
