namespace investmentSimuoation.Models.DTOs;

/// <summary>
/// Breakdown details of an investment projection.
/// </summary>
public class InvestmentSummaryBreakdown
{
    public decimal Principal { get; set; }
    public decimal EstimatedEarnings { get; set; }
    public decimal ManagementFee { get; set; }
    public decimal NetEstimatedPayout { get; set; }
}

/// <summary>
/// Calculated projection for an investment before confirmation.
/// </summary>
public class InvestmentSimulationResultDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    
    public decimal InvestedAmount { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public string ReturnRateDisplay { get; set; } = string.Empty;
    
    public int DurationDays { get; set; }
    public string DurationLabel { get; set; } = string.Empty;
    
    public decimal ExpectedReturnAmount { get; set; }
    public decimal TotalPayoutAtMaturity { get; set; }
    
    public DateTime StartDateUtc { get; set; }
    public DateTime MaturityDateUtc { get; set; }
    public string FormattedMaturityDate { get; set; } = string.Empty;
    
    public string Currency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    
    public string MaturityTerms { get; set; } = string.Empty;
    public string PayoutFrequency { get; set; } = "At Maturity";
    
    public InvestmentSummaryBreakdown SummaryBreakdown { get; set; } = new();
}
