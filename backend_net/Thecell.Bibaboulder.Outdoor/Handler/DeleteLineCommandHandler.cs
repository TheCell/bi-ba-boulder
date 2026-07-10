using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class DeleteLineCommandHandler : ICommandHandler<DeleteLineCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public DeleteLineCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(DeleteLineCommand command)
    {
        var line = await _dbContext.Lines
            .SingleOrDefaultAsync(l => l.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        await _dbContext.RemoveEntityAndSaveChangesAsync(line, command.Version);
    }
}