using KeyKeepers.DAL.Enums;

namespace KeyKeepers.BLL.DTOs.CommunityUsers;

public class CommunityUserResponseDto
{
    public CommunityRole Role { get; set; }

    public long UserId { get; set; }

    public long CommunityId { get; set; }
}
