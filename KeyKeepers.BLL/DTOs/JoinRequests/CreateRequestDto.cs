namespace KeyKeepers.BLL.DTOs.JoinRequests;

public class CreateRequestDto
{
    public string? Comment { get; set; }

    public long CommunityId { get; set; }

    public long RecipientId { get; set; }

    public long SenderId { get; set; }
}
