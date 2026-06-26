using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public partial class CreateSpraywallProblemCommandHandler : ICommandHandlerWithExtraTransaction<CreateSpraywallProblemCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISpraywallImageService _imageService;

    public CreateSpraywallProblemCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService,
        ISpraywallImageService imageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _imageService = imageService;
    }

    public async Task HandleAsync(CreateSpraywallProblemCommand command)
    {
        await _dbContext.Spraywalls
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == command.SpraywallId)
            .ThrowIfNullAsync(command.SpraywallId);

        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (!currentUser.IsInRole(UserRole.Editor))
        {
            throw new UnauthorizedAccessException("User does not have the required role.");
        }

        var editor = await _dbContext.Users
            .SingleAsync(u => u.Id == currentUser.Id);

        ValidateImage(command.Image);
        var imageBytes = ExtractImageBytes(command.Image);

        var problem = new SpraywallProblem
        {
            Id = Guid.CreateVersion7(),
            Name = command.Name.Trim(),
            Description = command.Description,
            FontGrade = command.FontGrade,
            CreatorId = editor.Id,
            SpraywallId = command.SpraywallId,
            IsCircuit = command.IsCircuit,
            NoMatch = command.NoMatch,
            FreeFeet = command.FreeFeet
        };

        await _dbContext.InsertEntityAndSaveChangesAsync(problem);
        await _imageService.SaveImageAsync(command.SpraywallId, problem.Id, imageBytes);

        command.Id = problem.Id;
    }

    private static void ValidateImage(string imageData)
    {
        if (string.IsNullOrEmpty(imageData))
        {
            throw new ArgumentException("Image is required");
        }

        if (!Base64PngRegex().IsMatch(imageData))
        {
            throw new ArgumentException("Image must be a valid base64 PNG string with data:image/png;base64, prefix");
        }
    }

    private static byte[] ExtractImageBytes(string imageData)
    {
        var match = Base64PngRegex().Match(imageData);
        var base64Data = match.Groups[1].Value;
        return Convert.FromBase64String(base64Data);
    }

    [GeneratedRegex(@"^data:image/png;base64,(.+)$")]
    private static partial Regex Base64PngRegex();
}
