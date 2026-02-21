using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateSectorCommandHandler : ICommandHandler<CreateSectorCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSectorCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateSectorCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserAsync();

        var testing = new Sector
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name,
            Description = command.Description,
            CreatedUserId = currentUser!.Id
        };

        await _dbContext.InsertEntityAsync(testing);
        command.Id = testing.Id;
    }
}
