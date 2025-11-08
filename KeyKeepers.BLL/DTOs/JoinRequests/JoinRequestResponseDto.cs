using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.DAL.Enums;
using KeyKeepers.BLL.DTOs.Users;

namespace KeyKeepers.BLL.DTOs.JoinRequests;

public class JoinRequestResponseDto
{
    public long Id { get; set; }

    public string? Comment { get; set; }

    public CommunityResponseDto Community { get; set; } = null!;

    public UserResponseDto Recipient { get; set; } = null!;

    public long SenderId { get; set; }

    public string SenderUsername { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public RequestStatus Status { get; set; }
}
