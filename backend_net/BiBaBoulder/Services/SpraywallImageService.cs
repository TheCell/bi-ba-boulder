using System;
using System.Threading.Tasks;
using SkiaSharp;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

public class SpraywallImageService : ISpraywallImageService
{
    private readonly IFileStorageClient _fileStorageClient;

    public SpraywallImageService(IFileStorageClient fileStorageClient)
    {
        _fileStorageClient = fileStorageClient;
    }

    public async Task SaveImageAsync(Guid spraywallId, Guid problemId, byte[] imageData)
    {
        var compressedData = CompressAndConvertTo24BitPng(imageData);
        var path = GetImagePath(spraywallId, problemId);
        await _fileStorageClient.WriteAsync(path, compressedData);
    }

    public async Task<string?> GetImageAsBase64Async(Guid spraywallId, Guid problemId)
    {
        var path = GetImagePath(spraywallId, problemId);
        var bytes = await _fileStorageClient.ReadAsync(path);

        if (bytes is null)
        {
            return null;
        }

        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }

    public async Task DeleteImageAsync(Guid spraywallId, Guid problemId)
    {
        var path = GetImagePath(spraywallId, problemId);
        await _fileStorageClient.DeleteAsync(path);
    }

    private static byte[] CompressAndConvertTo24BitPng(byte[] binaryData)
    {
        const int targetWidth = 128;
        const int targetHeight = 128;

        using var sourceImage = SKBitmap.Decode(binaryData);
        if (sourceImage == null)
        {
            throw new InvalidOperationException("Failed to decode image data");
        }

        // Create a new 24-bit bitmap (RGB, no alpha channel)
        var info = new SKImageInfo(targetWidth, targetHeight, SKColorType.Rgb888x, SKAlphaType.Opaque);
        using var newBitmap = new SKBitmap(info);

        using (var canvas = new SKCanvas(newBitmap))
        {
            canvas.Clear(SKColors.Black);
            var destRect = new SKRect(0, 0, targetWidth, targetHeight);
            canvas.DrawBitmap(sourceImage, destRect);
        }

        // Encode as PNG with maximum compression
        // Note: SkiaSharp doesn't expose PNG compression level directly,
        // but quality 100 with PNG format provides good compression
        using var image = SKImage.FromBitmap(newBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    private static string GetImagePath(Guid spraywallId, Guid problemId)
    {
        return $"spraywalls/{spraywallId}/{problemId}.png";
    }
}
