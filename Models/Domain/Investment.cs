namespace investmentSimuoation.Models.Domain;

/// <summary>
/// Represents a customer's simulated investment instance.
/// </summary>
public class Investment
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    
    public decimal InvestedAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public int DurationDays { get; set; }
    public string DurationLabel { get; set; } = string.Empty;
    
    public decimal ExpectedReturnAmount { get; set; }
    public decimal TotalPayoutAtMaturity { get; set; }
    
    public string Currency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    
    public string Status { get; set; } = "Active"; // Active, Matured, Cancelled
    public string InvestorName { get; set; } = "Demo Investor";
    public string FundingSource { get; set; } = "Primary Wallet";
    
    /// <summary>
    /// Client-supplied idempotency key or client request id to prevent duplicate submissions.
    /// </summary>
    public string? IdempotencyKey { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime MaturityDateUtc { get; set; }
}
