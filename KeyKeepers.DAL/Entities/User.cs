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

    public long TokenId { get; set; }

    public RefreshToken RefreshToken { get; set; } = new RefreshToken();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PrivateCategory> PrivateCategories { get; set; } = new List<PrivateCategory>();

    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
}
