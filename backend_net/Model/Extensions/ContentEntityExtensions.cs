using System;
using System.Collections.Generic;
using System.Linq;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;
using Thecell.Bibaboulder.Model.Model.Outdoor;

namespace Thecell.Bibaboulder.Model.Extensions;

public static class ContentEntityExtensions
{
    public static void UpdateContent(
        this IContentEntity contentEntity,
        string name,
        string? description,
        string? importantInfo,
        Uri? previewImageUri,
        ICollection<string> imageUris)
    {
        contentEntity.UpdateContent(name, description, importantInfo, previewImageUri?.AbsoluteUri, imageUris);
    }

    public static void UpdateContent(
        this IContentEntity contentEntity,
        string name,
        string? description,
        string? importantInfo,
        string? previewImageUri,
        ICollection<string> imageUris)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.");
        }

        ValidateImageUri(previewImageUri, nameof(previewImageUri));

        var images = imageUris
            .Select(uri => uri.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (images.Count != imageUris.Count)
        {
            throw new ArgumentException("Image URLs must be unique.");
        }

        foreach (var imageUri in images)
        {
            ValidateImageUri(imageUri, nameof(imageUris));
        }

        contentEntity.Name = name.Trim();
        contentEntity.Description = description;
        contentEntity.ImportantInfo = importantInfo;
        contentEntity.PreviewImageUri = previewImageUri?.Trim();
        contentEntity.Media.Clear();

        foreach (var imageUri in images)
        {
            // TODO this is rewriting all to image type.
            contentEntity.Media.Add(new PublicResource { Uri = imageUri, ResourceType = ResourceType.Image });
        }
    }

    private static void ValidateImageUri(string? uri, string parameterName)
    {
        if (uri is null)
        {
            return;
        }

        // TODO the uri must be relative.
        if (!Uri.TryCreate(uri.Trim(), UriKind.Absolute, out var parsedUri) ||
            (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Image URLs must be absolute HTTP(S) URLs.", parameterName);
        }
    }
}
