using Microsoft.AspNetCore.Http;

namespace RoomSplit.Core.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task DeleteFileAsync(string filePath);
    bool IsValidImage(IFormFile file);
}
