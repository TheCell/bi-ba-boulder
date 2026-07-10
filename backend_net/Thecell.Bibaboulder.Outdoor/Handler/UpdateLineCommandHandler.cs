using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class UpdateLineCommandHandler : ICommandHandler<UpdateLineCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public UpdateLineCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(UpdateLineCommand command)
    {
        var line = await _dbContext.Lines
            .SingleOrDefaultAsync(l => l.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        line.Identifier = command.Identifier.Trim();
        line.Name = command.Name?.Trim();
        line.Description = command.Description;
        line.FontGrade = command.FontGrade;
        line.Data = command.Data;

        await _dbContext.UpdateEntityAndSaveChangesAsync(line, command.Version);
    }
}