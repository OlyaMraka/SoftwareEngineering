namespace KeyKeepers.DAL.Entities;

public class Community
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CommunityUser> CommunityUsers { get; set; } = new List<CommunityUser>();

    public ICollection<BaseCategory> Categories { get; set; } = new List<BaseCategory>();

    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
}
