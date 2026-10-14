using System.ComponentModel.DataAnnotations;
using SimpleCrm.Api.Models;

namespace SimpleCrm.Api.Dtos;

public record DealDto(
    int Id,
    string Title,
    decimal Amount,
    DealStage Stage,
    DateOnly? ExpectedCloseDate,
    DateTime CreatedAt,
    int CustomerId,
    string CustomerName);

public record SaveDealDto(
    [Required, MaxLength(200)] string Title,
    [Range(0, 1_000_000_000)] decimal Amount,
    DealStage Stage,
    DateOnly? ExpectedCloseDate,
    [Range(1, int.MaxValue, ErrorMessage = "Välj en kund.")] int CustomerId);
