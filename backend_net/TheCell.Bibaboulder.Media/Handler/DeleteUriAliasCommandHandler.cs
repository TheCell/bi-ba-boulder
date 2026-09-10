using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Media.Handler;

public class DeleteUriAliasCommandHandler : ICommandHandler<DeleteUriAliasCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUriAliasCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(DeleteUriAliasCommand command)
    {
        await UriAliasHandlerUtilities.EnsureContentAdministratorAsync(_currentUserService);

        var uriAlias = await _dbContext.UriAliases
            .SingleOrDefaultAsync(uriAlias => uriAlias.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.RemoveEntityAndSaveChangesAsync(uriAlias, command.Version);
    }
}