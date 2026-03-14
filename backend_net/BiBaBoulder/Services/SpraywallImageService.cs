using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Thecell.Bibaboulder.Model.Services;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

public class SpraywallImageService : ISpraywallImageService
{
    private readonly string _basePath;

    public SpraywallImageService(IConfiguration configuration)
    {
        _basePath = configuration["SpraywallImageBasePath"] ?? "spraywalls";
    }

    public async Task SaveImageAsync(Guid spraywallId, Guid problemId, byte[] imageData)
    {
        var directory = Path.Combine(_basePath, spraywallId.ToString());
        Directory.CreateDirectory(directory);

        // todo compress image

        var filePath = Path.Combine(directory, $"{problemId}.png");
        await File.WriteAllBytesAsync(filePath, imageData);
    }

    public async Task<string?> GetImageAsBase64Async(Guid spraywallId, Guid problemId)
    {
        var filePath = Path.Combine(_basePath, spraywallId.ToString(), $"{problemId}.png");

        if (!File.Exists(filePath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(filePath);
        return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
    }

    public Task DeleteImageAsync(Guid spraywallId, Guid problemId)
    {
        var filePath = Path.Combine(_basePath, spraywallId.ToString(), $"{problemId}.png");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
