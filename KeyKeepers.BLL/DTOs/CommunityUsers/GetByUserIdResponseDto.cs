using KeyKeepers.DAL.Enums;
using KeyKeepers.BLL.DTOs.Communities;

namespace KeyKeepers.BLL.DTOs.CommunityUsers;

public class GetByUserIdResponseDto
{
    public long Id { get; set; }

    public CommunityResponseDto Community { get; set; } = null!;

    public CommunityRole UserRole { get; set; }
}
