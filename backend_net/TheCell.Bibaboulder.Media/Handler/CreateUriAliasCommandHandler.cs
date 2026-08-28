using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model.Media;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Media.Handler;

public class CreateUriAliasCommandHandler : ICommandHandler<CreateUriAliasCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateUriAliasCommandHandler(IBiBaBoulderDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateUriAliasCommand command)
    {
        await UriAliasHandlerUtilities.EnsureContentAdministratorAsync(_currentUserService);

        var normalizedAlias = UriAliasHandlerUtilities.NormalizeAlias(command.Alias);
        var type = UriAliasHandlerUtilities.GetUriType(command.TypeId);
        await UriAliasHandlerUtilities.ValidateTargetAsync(_dbContext, type, command.TargetId);
        await UriAliasHandlerUtilities.ValidateAliasIsUniqueAsync(_dbContext, normalizedAlias, type);

        var uriAlias = new UriAlias
        {
            Id = Guid.CreateVersion7(),
            Alias = normalizedAlias,
            Type = type
        };
        UriAliasHandlerUtilities.SetTarget(uriAlias, type, command.TargetId);

        await _dbContext.InsertEntityAndSaveChangesAsync(uriAlias);
        command.Id = uriAlias.Id;
    }
}
