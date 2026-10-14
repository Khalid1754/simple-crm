using System.ComponentModel.DataAnnotations;

namespace SimpleCrm.Api.Dtos;

// DTO = Data Transfer Object. Det är det vi skickar in och ut via API:t.
// Vi skickar inte entiteterna direkt, så att vi själva styr vilka fält som syns.

/// <summary>Det som API:t returnerar för en kund.</summary>
public record CustomerDto(
    int Id,
    string Name,
    string? OrgNumber,
    string? Email,
    string? Phone,
    string? City,
    DateTime CreatedAt,
    int ContactCount,
    int DealCount);

/// <summary>Det som klienten skickar när en kund skapas eller uppdateras.</summary>
public record SaveCustomerDto(
    [Required, MaxLength(200)] string Name,
    [MaxLength(20)] string? OrgNumber,
    [EmailAddress, MaxLength(200)] string? Email,
    [MaxLength(50)] string? Phone,
    [MaxLength(100)] string? City);
