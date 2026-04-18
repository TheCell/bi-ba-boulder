using System;
using System.Threading.Tasks;
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
        // todo compress image
        var path = GetImagePath(spraywallId, problemId);
        await _fileStorageClient.WriteAsync(path, imageData);
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

    private static string GetImagePath(Guid spraywallId, Guid problemId)
    {
        return $"spraywalls/{spraywallId}/{problemId}.png";
    }
}
