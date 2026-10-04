namespace PEWS.Services;

public interface IFileUploadService
{
    Task<string?> UploadImageAsync(IFormFile file, string folder);
    bool DeleteFile(string filePath);
    bool IsValidImage(IFormFile file);
}
