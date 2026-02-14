namespace RoomSplit.API.DTOs.Transaction;

public record TransactionImageDto(
    Guid Id,
    string ImageUrl,
    string? ThumbnailUrl,
    string OriginalFileName,
    long FileSize,
    DateTime UploadedAt);
