using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Services.Interfaces.Base;

namespace Wasenshi.HemoDialysisPro.Services.Interfaces
{
    public interface IUploadService : IApplicationService
    {
        Task<(byte[] bytes, string contentType)> Get(string fileUri);
        Task<string> Upload(IFormFile file, string id = null);
        bool DeleteFile(string id);

        IFormFile ResizeImage(IFormFile imageFile, int maxWidth, int maxHeight, SKFilterQuality quality = SKFilterQuality.Medium);
    }
}
