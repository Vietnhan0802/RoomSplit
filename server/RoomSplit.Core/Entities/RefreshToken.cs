using System;

namespace RoomSplit.Core.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty; // Hashed token
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; } // For token rotation
    public string? RevokedReason { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public string? RevokedByIp { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    // Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
