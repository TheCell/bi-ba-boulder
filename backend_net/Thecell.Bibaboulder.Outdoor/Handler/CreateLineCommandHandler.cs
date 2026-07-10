using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateLineCommandHandler : ICommandHandler<CreateLineCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public CreateLineCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(CreateLineCommand command)
    {
        if (command.Data.Positions.Count < 3)
        {
            throw new ArgumentException("A line must have at least 3 positions.");
        }

        await _dbContext.Blocs
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == command.BlocId)
            .ThrowIfNullAsync(command.BlocId);

        var line = new Line
        {
            Id = Guid.CreateVersion7(),
            BlocId = command.BlocId,
            Identifier = command.Identifier.Trim(),
            Name = command.Name?.Trim(),
            Description = command.Description,
            FontGrade = command.FontGrade,
            Data = command.Data
        };

        await _dbContext.InsertEntityAndSaveChangesAsync(line);
        command.Id = line.Id;
    }
}
