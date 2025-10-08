namespace KeyKeepers.BLL.DTOs.Users;

public class UserResponseDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;
}
