using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteOutdoorAreaCommandHandler : ICommandHandler<DeleteOutdoorAreaCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteOutdoorAreaCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeleteOutdoorAreaCommand command)
    {
        // todo add permission check
        var outdoorArea = await _dbContext.OutdoorAreas
            .SingleOrDefaultAsync(area => area.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.RemoveEntityAndSaveChangesAsync(outdoorArea, command.Version);
    }
}
