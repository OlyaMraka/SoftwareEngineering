namespace KeyKeepers.DAL.Entities;

public class RefreshToken
{
    public long Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public DateTime CreatedAt { get; set; }

    public DateTime? Revoked { get; set; }

    public bool IsActive => Revoked == null && !IsExpired;

    public long UserId { get; set; }

    public User User { get; set; } = null!;
}
