using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Thecell.Bibaboulder.Model;
using Thecell.Bibaboulder.Model.Authorization;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model.Media;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Media.Handler;

internal static partial class UriAliasHandlerUtilities
{
    public static async Task EnsureContentAdministratorAsync(ICurrentUserService currentUserService)
    {
        var currentUser = await currentUserService.GetCurrentUserOrThrowAsync();
        var canManageContent =
            currentUser.Roles.Contains(AuthorizationRoles.ContentAdmin) || currentUser.Roles.Contains(AuthorizationRoles.Admin);

        if (!canManageContent)
        {
            throw new UnauthorizedAccessException("Only content administrators can manage URI aliases.");
        }
    }

    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Public URI aliases are intentionally canonicalized to lowercase URL slugs.")]
    public static string NormalizeAlias(string alias)
    {
        var normalizedAlias = alias.Trim().ToLowerInvariant();
        if (normalizedAlias.Length > 100)
        {
            throw new ArgumentException("Alias cannot exceed 100 characters.");
        }

        if (!AliasRegex().IsMatch(normalizedAlias))
        {
            throw new ArgumentException("Alias must be a lowercase URL slug containing letters, numbers, and single hyphens.");
        }

        return normalizedAlias;
    }

    public static UriType GetUriType(long typeId)
    {
        if (typeId < int.MinValue || typeId > int.MaxValue || !Enum.IsDefined((UriType)(int)typeId))
        {
            throw new ArgumentException("URI alias type is invalid.");
        }

        return (UriType)(int)typeId;
    }

    public static async Task ValidateTargetAsync(IBiBaBoulderDbContext dbContext, UriType type, Guid targetId)
    {
        var targetExists = type switch
        {
            UriType.BoulderGym => await dbContext.BoulderGyms.AnyAsync(boulderGym => boulderGym.Id == targetId),
            UriType.OutdoorArea => await dbContext.OutdoorAreas.AnyAsync(outdoorArea => outdoorArea.Id == targetId),
            _ => throw new InvalidOperationException($"Unsupported URI alias type {type}.")
        };

        if (!targetExists)
        {
            throw new ArgumentException("The selected URI alias target does not exist.");
        }
    }

    public static async Task ValidateAliasIsUniqueAsync(
        IBiBaBoulderDbContext dbContext,
        string alias,
        UriType type,
        Guid? excludedId = null)
    {
        var aliasExists = await dbContext.UriAliases.AnyAsync(uriAlias =>
            uriAlias.Alias == alias &&
            uriAlias.Type == type &&
            (!excludedId.HasValue || uriAlias.Id != excludedId.Value));

        if (aliasExists)
        {
            throw new ArgumentException("This alias already exists for the selected type.");
        }
    }

    public static void SetTarget(UriAlias uriAlias, UriType type, Guid targetId)
    {
        uriAlias.BoulderGymId = type == UriType.BoulderGym ? targetId : null;
        uriAlias.OutdoorAreaId = type == UriType.OutdoorArea ? targetId : null;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex AliasRegex();
}
