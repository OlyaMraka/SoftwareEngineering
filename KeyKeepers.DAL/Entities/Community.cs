namespace KeyKeepers.DAL.Entities;

public class Community
{
    public long Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; } =  DateTime.Now;
    public ICollection<CommunityUser> CommunityUsers { get; set; } = new List<CommunityUser>();
    public ICollection<BaseCategory> Categories { get; set; } = new List<BaseCategory>();
}