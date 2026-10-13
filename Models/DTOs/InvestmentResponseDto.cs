using investmentSimuoation.Models.Domain;

namespace investmentSimuoation.Models.DTOs;

/// <summary>
/// Response returned upon creating or fetching an investment record.
/// </summary>
public class InvestmentResponseDto
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
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
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime MaturityDateUtc { get; set; }
    public string FormattedCreatedAt { get; set; } = string.Empty;
    public string FormattedMaturityDate { get; set; } = string.Empty;
    
    public string Currency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    
    public string InvestorName { get; set; } = string.Empty;
    public string FundingSource { get; set; } = string.Empty;
    public string? IdempotencyKey { get; set; }
    
    public bool WasDuplicatePrevented { get; set; }
    public string Message { get; set; } = "Investment created successfully.";

    public static InvestmentResponseDto FromDomain(Investment investment, bool wasDuplicatePrevented = false)
    {
        return new InvestmentResponseDto
        {
            ReferenceNumber = investment.ReferenceNumber,
            Status = investment.Status,
            ProductId = investment.ProductId,
            ProductName = investment.ProductName,
            Category = investment.Category,
            RiskLevel = investment.RiskLevel.ToString(),
            InvestedAmount = investment.InvestedAmount,
            AnnualInterestRate = investment.AnnualInterestRate,
            ReturnRateDisplay = $"{investment.AnnualInterestRate:0.##}% p.a.",
            DurationDays = investment.DurationDays,
            DurationLabel = investment.DurationLabel,
            ExpectedReturnAmount = investment.ExpectedReturnAmount,
            TotalPayoutAtMaturity = investment.TotalPayoutAtMaturity,
            CreatedAtUtc = investment.CreatedAtUtc,
            MaturityDateUtc = investment.MaturityDateUtc,
            FormattedCreatedAt = investment.CreatedAtUtc.ToString("MMM dd, yyyy HH:mm:ss 'UTC'"),
            FormattedMaturityDate = investment.MaturityDateUtc.ToString("MMM dd, yyyy"),
            Currency = investment.Currency,
            CurrencySymbol = investment.CurrencySymbol,
            InvestorName = investment.InvestorName,
            FundingSource = investment.FundingSource,
            IdempotencyKey = investment.IdempotencyKey,
            WasDuplicatePrevented = wasDuplicatePrevented,
            Message = wasDuplicatePrevented 
                ? "Existing investment returned (duplicate submission prevented)." 
                : "Investment created successfully."
        };
    }
}
