using System.Threading.Tasks;

namespace Thecell.Bibaboulder.BiBaBoulder.Services;

/// <summary>
/// Abstraction for file storage operations, allowing different storage providers (local disk, cloud, etc.).
/// </summary>
public interface IFileStorageClient
{
    /// <summary>
    /// Writes a file to storage.
    /// </summary>
    /// <param name="path">Relative path to the file (e.g., "spraywalls/guid/problem-guid.png").</param>
    /// <param name="data">File content as byte array.</param>
    Task WriteAsync(string path, byte[] data);

    /// <summary>
    /// Reads a file from storage.
    /// </summary>
    /// <param name="path">Relative path to the file.</param>
    /// <returns>File content as byte array, or null if the file doesn't exist.</returns>
    Task<byte[]?> ReadAsync(string path);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="path">Relative path to the file.</param>
    Task<bool> ExistsAsync(string path);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="path">Relative path to the file.</param>
    Task DeleteAsync(string path);
}
