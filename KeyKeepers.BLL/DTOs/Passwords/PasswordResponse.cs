namespace KeyKeepers.BLL.DTOs.Passwords;

public class PasswordResponse
{
    public long Id { get; set; }

    public string AppName { get; set; } = string.Empty;

    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    public long CategoryId { get; set; }
}
