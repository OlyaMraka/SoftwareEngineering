using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Enums;

namespace KeyKeepers.BLL.DTOs.CommunityUsers;

public class CommunityUserResponseDto
{
    public long Id { get; set; }

    public CommunityRole Role { get; set; }

    public long UserId { get; set; }

    public long CommunityId { get; set; }
}
