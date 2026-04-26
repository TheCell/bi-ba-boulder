using System;
using System.Threading.Tasks;

namespace Thecell.Bibaboulder.Model.Services;

public interface ISpraywallImageService
{
    Task SaveImageAsync(Guid spraywallId, Guid problemId, byte[] imageData);
    Task<string?> GetImageAsBase64Async(Guid spraywallId, Guid problemId);
    Task DeleteImageAsync(Guid spraywallId, Guid problemId);
}
