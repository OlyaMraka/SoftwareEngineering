using KeyKeepers.DAL.Enums;

namespace KeyKeepers.DAL.Entities;

public class JoinRequest
{
    public long Id { get; set; }

    public string? Comment { get; set; }

    public long CommunityId { get; set; }

    public Community Community { get; set; } = null!;

    public long RecipientId { get; set; }

    public User Recipient { get; set; } = null!;

    public long SenderId { get; set; }

    public CommunityUser Sender { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;
}
