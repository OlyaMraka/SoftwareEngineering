namespace KeyKeepers.DAL.Entities;

public class PrivateCategory : BaseCategory
{
    public long CommunityUserId { get; set; }
    public CommunityUser CommunityUser { get; set; } = new CommunityUser();
}