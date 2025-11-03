using KeyKeepers.DAL.Enums;

namespace KeyKeepers.DAL.Entities;

public class CommunityUser
{
    public long Id { get; set; }

    public CommunityRole Role { get; set; }

    public long UserId { get; set; }

    public User User { get; set; } = null!;

    public long CommunityId { get; set; }

    public Community Community { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
