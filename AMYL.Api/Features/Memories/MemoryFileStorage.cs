namespace AMYL.Api.Features.Memories;

public interface IFileStorage
{
    Task<string> UploadFile(IFormFile file, CancellationToken cancellationToken);
}

/// <summary>
/// Provider owned by the Memories feature - the only feature that stores files.
/// </summary>
public sealed class MemoryFileStorage : IFileStorage
{
    private readonly string _uploadPath;

    public MemoryFileStorage(IWebHostEnvironment environment)
    {
        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");
    }

    public async Task<string> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_uploadPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return fileName;
    }
}
