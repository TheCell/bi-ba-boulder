using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class UpdateBoulderGymCommandHandler : ICommandHandler<UpdateBoulderGymCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public UpdateBoulderGymCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(UpdateBoulderGymCommand command)
    {
        // todo add permission check
        var boulderGym = await _dbContext.BoulderGyms
            .SingleOrDefaultAsync(gym => gym.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        boulderGym.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);
        await _dbContext.UpdateEntityAndSaveChangesAsync(boulderGym, command.Version);
    }
}