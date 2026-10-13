using investmentSimuoation.Models.Domain;

namespace investmentSimuoation.Models.DTOs;

/// <summary>
/// Data Transfer Object representing an investment product.
/// </summary>
public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal AnnualInterestRate { get; set; }
    public string ReturnRateDisplay { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public string DurationLabel { get; set; } = string.Empty;
    public decimal MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public string MaturityTerms { get; set; } = string.Empty;
    public string PayoutFrequency { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = [];
    public bool IsActive { get; set; }
    
    public static ProductDto FromDomain(InvestmentProduct product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            RiskLevel = product.RiskLevel.ToString(),
            AnnualInterestRate = product.AnnualInterestRate,
            ReturnRateDisplay = $"{product.AnnualInterestRate:0.##}% p.a.",
            DurationDays = product.DurationDays,
            DurationLabel = product.DurationLabel,
            MinimumAmount = product.MinimumAmount,
            MaximumAmount = product.MaximumAmount,
            Currency = product.Currency,
            CurrencySymbol = product.CurrencySymbol,
            ShortDescription = product.ShortDescription,
            FullDescription = product.FullDescription,
            MaturityTerms = product.MaturityTerms,
            PayoutFrequency = product.PayoutFrequency,
            IconName = product.IconName,
            Highlights = product.Highlights,
            IsActive = product.IsActive
        };
    }
}
