using System.ComponentModel.DataAnnotations;

namespace investmentSimuoation.Models.DTOs;

/// <summary>
/// Request payload to preview / simulate return calculations before creating an investment.
/// </summary>
public class SimulateInvestmentRequest
{
    [Required(ErrorMessage = "Product ID is required.")]
    public string ProductId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Investment amount is required.")]
    [Range(0.01, 100_000_000, ErrorMessage = "Investment amount must be greater than zero.")]
    public decimal Amount { get; set; }
}
