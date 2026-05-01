using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

/// <summary>
/// Local file system implementation of file storage.
/// </summary>
public class LocalFileStorageClient : IFileStorageClient
{
    private readonly string _basePath;

    public LocalFileStorageClient(IConfiguration configuration)
    {
        _basePath = configuration["UserContentBasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "user-content");

        // Ensure base directory exists
        Directory.CreateDirectory(_basePath);
    }

    public async Task WriteAsync(string path, byte[] data)
    {
        var fullPath = GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(fullPath, data);
    }

    public async Task<byte[]?> ReadAsync(string path)
    {
        var fullPath = GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> ExistsAsync(string path)
    {
        var fullPath = GetFullPath(path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task DeleteAsync(string path)
    {
        var fullPath = GetFullPath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string relativePath)
    {
        // Normalize path separators and remove any leading slashes
        var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        return Path.Combine(_basePath, normalizedPath);
    }
}
