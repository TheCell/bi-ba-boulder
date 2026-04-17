using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Common.Exceptions;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Extensions;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.Spraywall.Handler;

public partial class UpdateSpraywallProblemCommandHandler : ICommandHandler<UpdateSpraywallProblemCommand>
{
    private readonly IBiBaBoulderDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISpraywallImageService _imageService;

    public UpdateSpraywallProblemCommandHandler(
        IBiBaBoulderDbContext dbContext,
        ICurrentUserService currentUserService,
        ISpraywallImageService imageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _imageService = imageService;
    }

    public async Task HandleAsync(UpdateSpraywallProblemCommand command)
    {
        var problem = await _dbContext.SpraywallProblems
            .SingleOrDefaultAsync(p => p.Id == command.Id)
            .ThrowIfNullAsync(command.Id);

        var currentUser = await _currentUserService.GetCurrentUserOrThrowAsync();

        if (currentUser.Id != problem.CreatorId)
        {
            throw new AccessDeniedException("Only the creator can update this problem");
        }

        if (!currentUser.IsInRole(UserRole.Editor))
        {
            throw new AccessDeniedException("Only users with editor role or higher can edit problems");
        }

        ValidateImage(command.Image);
        var imageBytes = ExtractImageBytes(command.Image);

        problem.Name = command.Name.Trim();
        problem.Description = command.Description;
        problem.FontGrade = command.FontGrade;

        await _dbContext.UpdateEntityAsync(problem, command.Version);
        await _imageService.SaveImageAsync(problem.SpraywallId, problem.Id, imageBytes);
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
