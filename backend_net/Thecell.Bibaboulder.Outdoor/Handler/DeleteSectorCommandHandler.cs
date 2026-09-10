using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteSectorCommandHandler : ICommandHandler<DeleteSectorCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteSectorCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeleteSectorCommand command)
    {
        // todo add permission check
        var sector = await _dbContext.Sectors
            .SingleOrDefaultAsync(item => item.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.RemoveEntityAndSaveChangesAsync(sector, command.Version);
    }
}
