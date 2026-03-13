using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thecell.Bibaboulder.Model.Services;

namespace TheCell.Bibaboulder.Sharedtests;

public class MockSpraywallImageService : ISpraywallImageService
{
    private readonly ConcurrentDictionary<string, byte[]> _images = new();

    public Task SaveImageAsync(Guid spraywallId, Guid problemId, byte[] imageData)
    {
        var key = GetKey(spraywallId, problemId);
        _images[key] = imageData;
        return Task.CompletedTask;
    }

    public Task<string> GetImageAsBase64Async(Guid spraywallId, Guid problemId)
    {
        var key = GetKey(spraywallId, problemId);
        if (_images.TryGetValue(key, out var imageBytes))
        {
            return Task.FromResult($"data:image/png;base64,{Convert.ToBase64String(imageBytes)}");
        }
        return Task.FromResult(string.Empty);
    }

    public Task DeleteImageAsync(Guid spraywallId, Guid problemId)
    {
        var key = GetKey(spraywallId, problemId);
        _images.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private static string GetKey(Guid spraywallId, Guid problemId) => $"{spraywallId}/{problemId}";
}
