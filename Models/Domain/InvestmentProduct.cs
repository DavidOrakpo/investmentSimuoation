namespace investmentSimuoation.Models.Domain;

/// <summary>
/// Represents an investment product available for customer simulation.
/// </summary>
public class InvestmentProduct
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    
    /// <summary>
    /// Annual interest rate percentage (e.g. 7.5 for 7.5% p.a.).
    /// </summary>
    public decimal AnnualInterestRate { get; set; }
    
    /// <summary>
    /// Investment tenure in days.
    /// </summary>
    public int DurationDays { get; set; }
    
    /// <summary>
    /// Human-readable duration label (e.g., "30 Days", "3 Months", "12 Months").
    /// </summary>
    public string DurationLabel { get; set; } = string.Empty;
    
    /// <summary>
    /// Minimum investment amount required in the product currency.
    /// </summary>
    public decimal MinimumAmount { get; set; }
    
    /// <summary>
    /// Optional maximum investment cap.
    /// </summary>
    public decimal? MaximumAmount { get; set; }
    
    /// <summary>
    /// Currency code (e.g. "USD").
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Currency symbol (e.g. "$").
    /// </summary>
    public string CurrencySymbol { get; set; } = "$";
    
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public string MaturityTerms { get; set; } = string.Empty;
    public string PayoutFrequency { get; set; } = "At Maturity";
    
    /// <summary>
    /// Suggested icon or badge name for mobile rendering (e.g. "shield", "savings", "trending_up").
    /// </summary>
    public string IconName { get; set; } = "account_balance";
    
    public List<string> Highlights { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
