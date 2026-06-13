using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BoulderLog.Handler;

public class CreateBoulderLogCommandHandler : ICommandHandler<CreateBoulderLogCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateBoulderLogCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(CreateBoulderLogCommand command)
    {
        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        var existingLog = await _dbContext.BoulderLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(bl => bl.UserId == currentUser.Id && bl.SpraywallProblemId == command.SpraywallProblemId);

        if (existingLog != null)
        {
            throw new InvalidOperationException("Log already exists");
        }

        var boulderLog = new Model.Model.BoulderLog
        {
            Id = Guid.CreateVersion7(),
            IsSent = command.IsSent,
            IsProject = command.IsProject,
            Rating = command.Rating,
            FontGrade = command.FontGrade,
            UserId = currentUser.Id,
            SpraywallProblemId = command.SpraywallProblemId
        };

        await _dbContext.InsertEntityAndSaveChangesAsync(boulderLog);
        command.Id = boulderLog.Id;
    }
}
