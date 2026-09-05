using System;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model.Indoor;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class CreateBoulderGymCommandHandler : ICommandHandler<CreateBoulderGymCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateBoulderGymCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateBoulderGymCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Admin) && !currentUser.IsInRole(UserRole.ContentAdmin))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

        var boulderGym = new BoulderGym { Id = Guid.CreateVersion7(), Name = command.Name, CreatedUserId = currentUser.Id };
        boulderGym.UpdateContent(command.Name, command.Description, command.ImportantInfo, command.PreviewImageUri, command.ImageUris);

        await _dbContext.InsertEntityAndSaveChangesAsync(boulderGym);
        command.Id = boulderGym.Id;
    }
}
