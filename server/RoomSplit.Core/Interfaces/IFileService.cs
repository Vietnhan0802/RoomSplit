using Microsoft.AspNetCore.Http;

namespace RoomSplit.Core.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task<(string imageUrl, string thumbnailUrl)> UploadWithThumbnailAsync(IFormFile file, string folder);
    Task DeleteFileAsync(string filePath);
    Task DeleteFilesAsync(IEnumerable<string> filePaths);
    bool IsValidImage(IFormFile file);
}
