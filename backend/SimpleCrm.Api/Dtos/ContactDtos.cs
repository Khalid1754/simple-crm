using System.ComponentModel.DataAnnotations;

namespace SimpleCrm.Api.Dtos;

public record ContactDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? JobTitle,
    int CustomerId,
    string CustomerName);

public record SaveContactDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [EmailAddress, MaxLength(200)] string? Email,
    [MaxLength(50)] string? Phone,
    [MaxLength(100)] string? JobTitle,
    [Range(1, int.MaxValue, ErrorMessage = "Välj en kund.")] int CustomerId);
