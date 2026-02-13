using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateSectorCommandHandler : ICommandHandler<CreateSectorCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;

    public CreateSectorCommandHandler(IBiBaBoulderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(CreateSectorCommand command)
    {
        var testing = new Sector
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Description = command.Description
        };

        await _dbContext.InsertEntityAsync(testing);
        command.Id = testing.Id;
    }
}
