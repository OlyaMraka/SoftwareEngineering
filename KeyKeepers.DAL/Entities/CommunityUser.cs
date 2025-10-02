using KeyKeepers.DAL.Enums;

namespace KeyKeepers.DAL.Entities;

public class CommunityUser
{
    public long Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public CommunityRole Role { get; set; }

    public long UserId { get; set; }

    public User User { get; set; } = new User();

    public long CommunityId { get; set; }

    public Community Community { get; set; } = new Community();

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<PrivateCategory> PrivateCategories { get; set; } = new List<PrivateCategory>();
}
