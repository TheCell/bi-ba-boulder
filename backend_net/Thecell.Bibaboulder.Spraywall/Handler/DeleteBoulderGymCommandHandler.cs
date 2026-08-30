using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class DeleteBoulderGymCommandHandler : ICommandHandler<DeleteBoulderGymCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteBoulderGymCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeleteBoulderGymCommand command)
    {
        // todo add permission check
        var boulderGym = await _dbContext.BoulderGyms
            .SingleOrDefaultAsync(gym => gym.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.RemoveEntityAndSaveChangesAsync(boulderGym, command.Version);
    }
}