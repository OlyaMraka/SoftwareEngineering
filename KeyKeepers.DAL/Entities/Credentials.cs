namespace KeyKeepers.DAL.Entities;

public class Credentials
{
    public long Id { get; set; }
    public string AppName { get; set; } = String.Empty;
    public string Login { get; set; } = String.Empty;
    public string PasswordHash { get; set; } = String.Empty;
    public string LogoUrl { get; set; } = String.Empty;
    public long CategoryId { get; set; }
    public BaseCategory Category { get; set; } = new BaseCategory();
}