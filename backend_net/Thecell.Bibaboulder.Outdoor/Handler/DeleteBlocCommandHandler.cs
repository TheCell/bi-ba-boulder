using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteBlocCommandHandler : ICommandHandler<DeleteBlocCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteBlocCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeleteBlocCommand command)
    {
        // AdditionalParts are loaded so EF detaches them (FK set to null) instead of violating the self-reference.
        var bloc = await _dbContext.Blocs
            .Include(item => item.AdditionalParts)
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.RemoveEntityAndSaveChangesAsync(bloc, command.Version);
    }
}
