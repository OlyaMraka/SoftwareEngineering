using KeyKeepers.BLL.DTOs.CommunityUsers;

namespace KeyKeepers.BLL.DTOs.Communities;

public class CreateCommunityResponseDto
{
    public CommunityResponseDto NewCommunity { get; set; } = null!;

    public CommunityUserResponseDto Owner { get; set; } = null!;
}
