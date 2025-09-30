namespace KeyKeepers.DAL.Entities;

public class BaseCategory
{
    public long Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public long CommunityId { get; set; }
    public Community Community { get; set; } = new  Community();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<Credentials> CredentialsCollection { get; set; } = new List<Credentials>();
}