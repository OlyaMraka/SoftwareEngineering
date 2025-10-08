namespace KeyKeepers.BLL.DTOs.Users;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public UserResponseDto UserResponseDto { get; set; } = new UserResponseDto();
}
