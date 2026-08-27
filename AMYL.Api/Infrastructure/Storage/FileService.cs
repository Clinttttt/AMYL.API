using AMYL.Api.Features.Memories;

namespace AMYL.Api.Infrastructure.Storage
{
    public sealed class FileService : IFileService
    {
        public readonly string _uploadPath;
        public FileService(IWebHostEnvironment environment)
        {
            _uploadPath = Path.Combine(
                environment.WebRootPath, "uploads");
        }

        public async Task<string> UploadFile(IFormFile image, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_uploadPath);
            var extension = Path.GetExtension(image.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);

            await image.CopyToAsync(stream, cancellationToken);
            return fileName;
        }


    }
}
