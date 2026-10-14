namespace SimpleCrm.Api.Models;

/// <summary>
/// En kund, t.ex. ett företag som vi gör affärer med.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OrgNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties: EF Core fyller dessa när vi använder .Include(...)
    public List<Contact> Contacts { get; set; } = new();
    public List<Deal> Deals { get; set; } = new();
}
