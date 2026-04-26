using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thecell.Bibaboulder.BiBaBoulder.Services;

namespace TheCell.Bibaboulder.Sharedtests;

/// <summary>
/// In-memory implementation of file storage for testing.
/// </summary>
public class MockFileStorageClient : IFileStorageClient
{
    private readonly ConcurrentDictionary<string, byte[]> _files = new();

    public Task WriteAsync(string path, byte[] data)
    {
        _files[path] = data;
        return Task.CompletedTask;
    }

    public Task<byte[]?> ReadAsync(string path)
    {
        _files.TryGetValue(path, out var data);
        return Task.FromResult(data);
    }

    public Task<bool> ExistsAsync(string path)
    {
        return Task.FromResult(_files.ContainsKey(path));
    }

    public Task DeleteAsync(string path)
    {
        _files.TryRemove(path, out _);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _files.Clear();
    }

    public int Count => _files.Count;
}
