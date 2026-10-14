namespace SimpleCrm.Api.Models;

/// <summary>
/// En kontaktperson som jobbar hos en kund.
/// </summary>
public class Contact
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }

    // Främmande nyckel (foreign key) till kunden
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
