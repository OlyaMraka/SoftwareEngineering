namespace KeyKeepers.DAL.Entities;

public class Credentials
{
    public long Id { get; set; }

    public string AppName { get; set; } = string.Empty;

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string LogoUrl { get; set; } = string.Empty;

    public long CategoryId { get; set; }

    public BaseCategory Category { get; set; } = new BaseCategory();
}
