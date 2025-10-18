namespace KeyKeepers.BLL.DTOs.Passwords;

public class CreatePasswordRequest
{
    public string AppName { get; set; } = string.Empty;

    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public long CategoryId { get; set; }
}
