namespace SimpleCrm.Api.Models;

/// <summary>
/// En affär (säljmöjlighet) kopplad till en kund.
/// </summary>
public class Deal
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DealStage Stage { get; set; } = DealStage.Lead;
    public DateOnly? ExpectedCloseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

/// <summary>
/// Vilket steg i säljprocessen affären befinner sig i.
/// Sparas som text i databasen (se CrmDbContext) så att den är lätt att läsa i SSMS.
/// </summary>
public enum DealStage
{
    Lead,
    Contacted,
    Proposal,
    Won,
    Lost
}
