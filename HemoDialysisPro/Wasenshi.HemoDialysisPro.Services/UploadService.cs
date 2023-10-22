using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Repositories.Interfaces;
using Wasenshi.HemoDialysisPro.Services.Interfaces;
using Wasenshi.HemoDialysisPro.Models;
using Wasenshi.HemoDialysisPro.Utils;

namespace Wasenshi.HemoDialysisPro.Services
{
    public class UploadService : IUploadService
    {
        private readonly IFileRepository fileRepo;
        private readonly IWebHostEnvironment webHost;
        private readonly ILogger<UploadService> logger;
        public readonly string uploadsFolder = Path.Combine(
#if DEBUG
            AppDomain.CurrentDomain.BaseDirectory

#else
            Environment.CurrentDirectory
#endif
            , "upload");

        public UploadService(IFileRepository fileRepo, IWebHostEnvironment webHost, ILogger<UploadService> logger)
        {
            this.fileRepo = fileRepo;
            this.webHost = webHost;
            this.logger = logger;
        }

        public async Task<(byte[] bytes, string contentType)> Get(string fileUri)
        {
            FileEntry file = fileRepo.Get(fileUri);
            if (file == null)
            {
                return (null, null);
            }
            var uri = Path.Combine(uploadsFolder, file.Uri);
            logger.LogDebug("Get file: {uri}", uri);

            var bytes = await File.ReadAllBytesAsync(uri);
            return (bytes, file.ContentType);
        }

        public async Task<string> Upload(IFormFile file, string id = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N");
            }

            string fileName = id + "." + file.ContentType.Split('/').Last().Split('+').First();

            FileEntry old = fileRepo.Get(id);
            if (old != null)
            {
                File.Delete(Path.Combine(uploadsFolder, old.Uri));
            }

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string uri = Path.Combine(uploadsFolder, fileName);
            logger.LogDebug("Upload file: {uri}", uri);
            await using (var stream = new FileStream(uri, FileMode.OpenOrCreate))
            {
                await file.OpenReadStream().CopyToAsync(stream);
            }

            if (old != null)
            {
                old.Uri = fileName;
                old.ContentType = file.ContentType;
                fileRepo.Update(old);
            }
            else
            {
                fileRepo.Insert(new FileEntry { Id = id, Uri = fileName, ContentType = file.ContentType });
            }

            fileRepo.Complete();

            return id;
        }

        public bool DeleteFile(string id)
        {
            FileEntry old = fileRepo.Get(id);
            if (old != null)
            {
                File.Delete(Path.Combine(uploadsFolder, old.Uri));
                fileRepo.Delete(old);
                fileRepo.Complete();
                return true;
            }

            return false;
        }

        public IFormFile ResizeImage(IFormFile imageFile, int maxWidth, int maxHeight,
                SKFilterQuality quality = SKFilterQuality.Medium)
        {
            if (imageFile.ContentType.Contains("svg") || imageFile.ContentType.Contains("vector"))
            {
                return imageFile;
            }
            var format = GetImageFormat(imageFile.ContentType);
            using SKBitmap sourceBitmap = SKBitmap.Decode(imageFile.OpenReadStream());

            using SKBitmap scaledBitmap = ScaleDownImage(sourceBitmap, maxWidth, maxHeight, quality);
            SKData data = SKImage.FromBitmap(scaledBitmap).Encode(format, ((int)quality * 25) + 25); // encode quality level is 0 - 100

            var result = new FormFile(data.AsStream(), 0, data.Size, imageFile.Name, imageFile.FileName)
            {
                Headers = imageFile.Headers,
                ContentType = imageFile.ContentType,
                ContentDisposition = imageFile.ContentDisposition
            };

            return result;
        }

        public static SKEncodedImageFormat GetImageFormat(string contentType)
        {
            string type = contentType.Split('/').Last().ToLower();

            return type switch
            {
                "jpeg" or "jpg" => SKEncodedImageFormat.Jpeg,
                "png" => SKEncodedImageFormat.Png,
                "bitmap" or "bmp" => SKEncodedImageFormat.Bmp,
                _ => throw new AppException("FORMAT", "Unsupported format."),
            };
        }

        public static SKBitmap ScaleDownImage(SKBitmap image, int maxWidth, int maxHeight, SKFilterQuality quality = SKFilterQuality.Medium)
        {
            if (image.Width <= maxWidth && image.Height <= maxHeight)
            {
                return image;
            }

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            SKBitmap scaledBitmap = image.Resize(new SKImageInfo(newWidth, newHeight), quality);

            return scaledBitmap;
        }
    }
}
