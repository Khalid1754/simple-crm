namespace SimpleCrm.Api.Models;

/// <summary>
/// En användare som kan logga in i CRM:et.
/// Vi sparar aldrig lösenordet i klartext, bara en hash.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
