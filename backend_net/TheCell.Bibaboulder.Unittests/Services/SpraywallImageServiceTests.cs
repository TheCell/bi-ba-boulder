using System;
using System.Threading.Tasks;
using Moq;
using SkiaSharp;
using Thecell.Bibaboulder.BiBaBoulder.Services;

namespace TheCell.Bibaboulder.Unittests.Services;

public class SpraywallImageServiceTests
{
    private readonly Mock<IFileStorageClient> _mockFileStorage;
    private readonly SpraywallImageService _service;

    public SpraywallImageServiceTests()
    {
        _mockFileStorage = new Mock<IFileStorageClient>();
        _service = new SpraywallImageService(_mockFileStorage.Object);
    }

    private static byte[] CreateValidPngImage()
    {
        // Create a simple 10x10 red image for testing
        var info = new SKImageInfo(10, 10);
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    [Fact]
    public async Task SaveImageAsync_CallsFileStorageWithCorrectPath()
    {
        var spraywallId = Guid.CreateVersion7();
        var problemId = Guid.CreateVersion7();
        var imageData = CreateValidPngImage();
        var expectedPath = $"spraywalls/{spraywallId}/{problemId}.png";

        await _service.SaveImageAsync(spraywallId, problemId, imageData);

        // Verify the path is correct; the data will be compressed so we use It.IsAny
        _mockFileStorage.Verify(
            x => x.WriteAsync(expectedPath, It.IsAny<byte[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetImageAsBase64Async_ReturnsNullWhenFileDoesNotExist()
    {
        var spraywallId = Guid.CreateVersion7();
        var problemId = Guid.CreateVersion7();
        _mockFileStorage.Setup(x => x.ReadAsync(It.IsAny<string>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _service.GetImageAsBase64Async(spraywallId, problemId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetImageAsBase64Async_ReturnsBase64StringWhenFileExists()
    {
        var spraywallId = Guid.CreateVersion7();
        var problemId = Guid.CreateVersion7();
        var imageData = CreateValidPngImage();
        var expectedPath = $"spraywalls/{spraywallId}/{problemId}.png";
        var expectedBase64 = $"data:image/png;base64,{Convert.ToBase64String(imageData)}";

        _mockFileStorage.Setup(x => x.ReadAsync(expectedPath))
            .ReturnsAsync(imageData);

        var result = await _service.GetImageAsBase64Async(spraywallId, problemId);

        Assert.NotNull(result);
        Assert.Equal(expectedBase64, result);
    }

    [Fact]
    public async Task DeleteImageAsync_CallsFileStorageWithCorrectPath()
    {
        var spraywallId = Guid.CreateVersion7();
        var problemId = Guid.CreateVersion7();
        var expectedPath = $"spraywalls/{spraywallId}/{problemId}.png";

        await _service.DeleteImageAsync(spraywallId, problemId);

        _mockFileStorage.Verify(
            x => x.DeleteAsync(expectedPath),
            Times.Once);
    }

    [Fact]
    public async Task SaveImageAsync_UsesConsistentPathFormat()
    {
        var spraywallId = Guid.CreateVersion7();
        var problemId = Guid.CreateVersion7();
        var imageData = CreateValidPngImage();

        await _service.SaveImageAsync(spraywallId, problemId, imageData);
        await _service.GetImageAsBase64Async(spraywallId, problemId);
        await _service.DeleteImageAsync(spraywallId, problemId);

        var expectedPath = $"spraywalls/{spraywallId}/{problemId}.png";
        _mockFileStorage.Verify(x => x.WriteAsync(expectedPath, It.IsAny<byte[]>()), Times.Once);
        _mockFileStorage.Verify(x => x.ReadAsync(expectedPath), Times.Once);
        _mockFileStorage.Verify(x => x.DeleteAsync(expectedPath), Times.Once);
    }
}
