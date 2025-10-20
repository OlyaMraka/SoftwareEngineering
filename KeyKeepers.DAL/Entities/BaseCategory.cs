namespace KeyKeepers.DAL.Entities;

public class BaseCategory
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public long? CommunityId { get; set; }

    public Community? Community { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Credentials> CredentialsCollection { get; set; } = new List<Credentials>();
}
