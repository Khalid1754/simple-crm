using System.ComponentModel.DataAnnotations;

namespace SimpleCrm.Api.Dtos;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RegisterDto(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6, ErrorMessage = "Lösenordet måste vara minst 6 tecken.")] string Password);

/// <summary>Svaret vid lyckad inloggning. Token skickas sedan med i varje anrop.</summary>
public record AuthResponseDto(string Token, string Email, DateTime ExpiresAt);
