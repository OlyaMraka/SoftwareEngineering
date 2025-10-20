namespace KeyKeepers.DAL.Entities;

public class PrivateCategory : BaseCategory
{
    public long UserId { get; set; }

    public User User { get; set; } = null!;
}
