namespace KeyKeepers.BLL.DTOs.Communities;

public class UpdateCommunityRequestDto
{
    public long CommunityId { get; set; }

    public string Name { get; set; } = null!;
}
