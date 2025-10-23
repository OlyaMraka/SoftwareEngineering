namespace KeyKeepersClient;

/// <summary>
/// Helper class to store password data.
/// </summary>
public class PasswordData
{
    /// <summary>
    /// Gets or sets the password ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the password entry.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the login/username.
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password value.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon path.
    /// </summary>
    public string IconPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password strength (strong, medium, weak, or none).
    /// </summary>
    public string Strength { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public long CategoryId { get; set; }
}
