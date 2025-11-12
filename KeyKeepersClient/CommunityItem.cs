using KeyKeepers.DAL.Enums;

namespace KeyKeepersClient;

public class CommunityItem
{
    public long CommunityId { get; set; }

    public long CommunityUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CommunityRole UserRole { get; set; }
}
