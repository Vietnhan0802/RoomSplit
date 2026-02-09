using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoomSplit.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace RoomSplit.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;

    public FileService(IConfiguration configuration)
    {
        _uploadPath = configuration["Upload:Path"] ?? "./uploads";
        _maxFileSize = (long.Parse(configuration["Upload:MaxFileSizeMB"] ?? "5")) * 1024 * 1024;
        _allowedExtensions = (configuration["Upload:AllowedTypes"] ?? ".jpg,.jpeg,.png,.webp")
            .Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        if (file.Length > _maxFileSize)
            throw new InvalidOperationException($"File size exceeds the limit of {_maxFileSize / 1024 / 1024}MB.");

        if (!IsValidImage(file))
            throw new InvalidOperationException("Invalid file type. Only JPG, PNG, and WebP are allowed.");

        var folderPath = Path.Combine(_uploadPath, folder);
        Directory.CreateDirectory(folderPath);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());

        // Resize if too large (max 1920px width)
        if (image.Width > 1920)
        {
            image.Mutate(x => x.Resize(1920, 0));
        }

        await image.SaveAsync(filePath);

        return $"/{folder}/{fileName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public bool IsValidImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
}
