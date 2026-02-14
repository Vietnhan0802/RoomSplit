using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RoomSplit.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
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

    public async Task<(string imageUrl, string thumbnailUrl)> UploadWithThumbnailAsync(IFormFile file, string folder)
    {
        if (file.Length > _maxFileSize)
            throw new InvalidOperationException($"File size exceeds the limit of {_maxFileSize / 1024 / 1024}MB.");

        if (!IsValidImage(file))
            throw new InvalidOperationException("Invalid file type. Only JPG, PNG, and WebP are allowed.");

        var folderPath = Path.Combine(_uploadPath, folder);
        var thumbFolderPath = Path.Combine(_uploadPath, folder, "thumbs");
        Directory.CreateDirectory(folderPath);
        Directory.CreateDirectory(thumbFolderPath);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);
        var thumbFilePath = Path.Combine(thumbFolderPath, fileName);

        var encoder = new JpegEncoder { Quality = 80 };

        // Save main image (max 1200px width)
        using (var image = await Image.LoadAsync(file.OpenReadStream()))
        {
            if (image.Width > 1200)
            {
                image.Mutate(x => x.Resize(1200, 0));
            }
            await image.SaveAsync(filePath, encoder);
        }

        // Generate thumbnail (200x200 center crop)
        using (var thumbImage = await Image.LoadAsync(file.OpenReadStream()))
        {
            thumbImage.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(200, 200),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center
            }));
            await thumbImage.SaveAsync(thumbFilePath, encoder);
        }

        return ($"/{folder}/{fileName}", $"/{folder}/thumbs/{fileName}");
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

    public Task DeleteFilesAsync(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            var fullPath = Path.Combine(_uploadPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        return Task.CompletedTask;
    }

    public bool IsValidImage(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
}
