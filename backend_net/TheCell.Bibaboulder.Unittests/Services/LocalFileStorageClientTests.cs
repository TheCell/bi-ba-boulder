using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Thecell.Bibaboulder.BiBaBoulder.Services;

namespace TheCell.Bibaboulder.Unittests.Services;

public class LocalFileStorageClientTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalFileStorageClient _storage;

    public LocalFileStorageClientTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"test-storage-{Guid.NewGuid()}");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new System.Collections.Generic.KeyValuePair<string, string?>("FileStorageBasePath", _testBasePath)
            ])
            .Build();

        _storage = new LocalFileStorageClient(configuration);
    }

    [Fact]
    public async Task WriteAsync_CreatesFileWithCorrectContent()
    {
        var path = "test/file.txt";
        var data = System.Text.Encoding.UTF8.GetBytes("Hello, World!");

        await _storage.WriteAsync(path, data);

        var fullPath = Path.Combine(_testBasePath, path);
        Assert.True(File.Exists(fullPath));
        var content = File.ReadAllBytes(fullPath);
        Assert.Equal(data, content);
    }

    [Fact]
    public async Task WriteAsync_CreatesNestedDirectories()
    {
        var path = "level1/level2/level3/file.txt";
        var data = (byte[])[1, 2, 3];

        await _storage.WriteAsync(path, data);

        var fullPath = Path.Combine(_testBasePath, path);
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task ReadAsync_ReturnsNullForNonExistentFile()
    {
        var result = await _storage.ReadAsync("does-not-exist.txt");

        Assert.Null(result);
    }

    [Fact]
    public async Task ReadAsync_ReturnsCorrectContent()
    {
        var path = "test-read.txt";
        var data = System.Text.Encoding.UTF8.GetBytes("Test content");
        await _storage.WriteAsync(path, data);

        var result = await _storage.ReadAsync(path);

        Assert.NotNull(result);
        Assert.Equal(data, result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingFile()
    {
        var path = "exists.txt";
        await _storage.WriteAsync(path, [1]);

        var result = await _storage.ExistsAsync(path);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalseForNonExistentFile()
    {
        var result = await _storage.ExistsAsync("does-not-exist.txt");

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile()
    {
        var path = "to-delete.txt";
        await _storage.WriteAsync(path, [1]);

        await _storage.DeleteAsync(path);

        var exists = await _storage.ExistsAsync(path);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrowForNonExistentFile()
    {
        await _storage.DeleteAsync("does-not-exist.txt");
    }

    [Fact]
    public async Task WriteAsync_NormalizesPathSeparators()
    {
        var path = "folder/subfolder\\file.txt";
        var data = (byte[])[1, 2, 3];

        await _storage.WriteAsync(path, data);

        var result = await _storage.ReadAsync("folder/subfolder/file.txt");
        Assert.NotNull(result);
        Assert.Equal(data, result);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, recursive: true);
        }
        GC.SuppressFinalize(this);
    }
}
