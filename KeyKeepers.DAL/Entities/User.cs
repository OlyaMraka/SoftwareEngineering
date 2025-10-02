namespace KeyKeepers.DAL.Entities;

public class User
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public ICollection<CommunityUser> CommunityUsers { get; set; } = new List<CommunityUser>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
