namespace KeyKeepers.DAL.Entities;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Surname { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string UserName { get; set; } = String.Empty;
    public string PasswordHash { get; set; } = String.Empty;
    public string PhotoUrl { get; set; } = String.Empty;
    public ICollection<CommunityUser> CommunityUsers { get; set; } = new List<CommunityUser>();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}