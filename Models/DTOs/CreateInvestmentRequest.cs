using System.ComponentModel.DataAnnotations;

namespace investmentSimuoation.Models.DTOs;

/// <summary>
/// Request payload to create/simulate an investment.
/// </summary>
public class CreateInvestmentRequest
{
    [Required(ErrorMessage = "Product ID is required.")]
    public string ProductId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Investment amount is required.")]
    [Range(0.01, 100_000_000, ErrorMessage = "Investment amount must be greater than zero.")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Optional customer display name (defaults to "Demo Customer").
    /// </summary>
    public string? InvestorName { get; set; }

    /// <summary>
    /// Source of funds for simulation display (e.g. "Main Wallet", "Bank Transfer").
    /// </summary>
    public string? FundingSource { get; set; }

    /// <summary>
    /// Optional idempotency token to prevent duplicate submissions from rapid tapping or network retries.
    /// Can also be passed in the HTTP request header 'Idempotency-Key'.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
