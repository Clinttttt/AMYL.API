namespace AMYL.Api.Features.Memories
{
    public interface IFileService
    {
        Task<string> UploadFile(IFormFile image, CancellationToken cancellationToken);
    }
}
