namespace KeyKeepers.BLL.DTOs.Users;

public class UserLogInResponseDto
{
    public long Id { get; set; }

    public required string AccessToken { get; set; }
}
