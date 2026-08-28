using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Media.Handler;

public class UpdateUriAliasCommandHandler : ICommandHandler<UpdateUriAliasCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUriAliasCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(UpdateUriAliasCommand command)
    {
        await UriAliasHandlerUtilities.EnsureContentAdministratorAsync(_currentUserService);

        var uriAlias = await _dbContext.UriAliases
            .SingleOrDefaultAsync(uriAlias => uriAlias.Id == command.Id)
            .ThrowIfNullAsync(command.Id);
        var normalizedAlias = UriAliasHandlerUtilities.NormalizeAlias(command.Alias);
        var type = UriAliasHandlerUtilities.GetUriType(command.TypeId);
        await UriAliasHandlerUtilities.ValidateTargetAsync(_dbContext, type, command.TargetId);
        await UriAliasHandlerUtilities.ValidateAliasIsUniqueAsync(_dbContext, normalizedAlias, type);

        uriAlias.Alias = normalizedAlias;
        uriAlias.Type = type;
        UriAliasHandlerUtilities.SetTarget(uriAlias, type, command.TargetId);

        await _dbContext.UpdateEntityAndSaveChangesAsync(uriAlias, command.Version);
    }
}
