using investmentSimuoation.Models.Domain;

namespace investmentSimuoation.Services;

public class ProductService : IProductService
{
    private static readonly List<InvestmentProduct> SeedProducts =
    [
        new InvestmentProduct
        {
            Id = "treasury-yield-30d",
            Name = "Federal Treasury 30-Day Note",
            Category = "Government Bonds",
            RiskLevel = RiskLevel.Low,
            AnnualInterestRate = 7.50m,
            DurationDays = 30,
            DurationLabel = "30 Days",
            MinimumAmount = 100.00m,
            MaximumAmount = 250_000.00m,
            Currency = "USD",
            CurrencySymbol = "$",
            ShortDescription = "Backed by sovereign treasury assets with guaranteed interest upon maturity.",
            FullDescription = "The Federal Treasury 30-Day Note is a short-term, sovereign-backed investment vehicle engineered for capital preservation and liquid capital allocation. Backed directly by government debt reserves, it provides fixed daily accrual with guaranteed redemption at tenure completion.",
            MaturityTerms = "Principal and accumulated interest will be automatically settled into your primary funding account on the 30th day. Zero penalty applies after maturity.",
            PayoutFrequency = "At Maturity",
            IconName = "account_balance",
            Highlights =
            [
                "Zero capital loss guarantee",
                "Daily interest accrual calculation",
                "Automated settlement upon maturity",
                "Ideal for short-term emergency reserve growth"
            ]
        },
        new InvestmentProduct
        {
            Id = "fixed-deposit-90d",
            Name = "Secure 90-Day Fixed Deposit",
            Category = "Fixed Income",
            RiskLevel = RiskLevel.Low,
            AnnualInterestRate = 9.80m,
            DurationDays = 90,
            DurationLabel = "90 Days (3 Months)",
            MinimumAmount = 250.00m,
            MaximumAmount = 500_000.00m,
            Currency = "USD",
            CurrencySymbol = "$",
            ShortDescription = "Competitive quarterly lock-in returns with full principal protection.",
            FullDescription = "Our 90-Day Fixed Deposit locks in an attractive 9.80% annualized return over a 3-month duration. It delivers predictable yields, compounding protection, and institutional-grade safety for your savings.",
            MaturityTerms = "Upon the 90-day maturity date, the full principal and total earned interest are transferred to your wallet balance. Flexible rollover options are available in your account settings.",
            PayoutFrequency = "At Maturity",
            IconName = "savings",
            Highlights =
            [
                "Protected fixed return rate",
                "Quarterly investment horizon",
                "Automatic credit of principal and yield",
                "Low minimum entry barrier ($250)"
            ]
        },
        new InvestmentProduct
        {
            Id = "green-energy-bond-180d",
            Name = "Clean Energy Infrastructure Bond",
            Category = "Renewable Infrastructure",
            RiskLevel = RiskLevel.Medium,
            AnnualInterestRate = 13.20m,
            DurationDays = 180,
            DurationLabel = "180 Days (6 Months)",
            MinimumAmount = 500.00m,
            MaximumAmount = 1_000_000.00m,
            Currency = "USD",
            CurrencySymbol = "$",
            ShortDescription = "Finance verified solar and wind projects while generating above-market yields.",
            FullDescription = "The Clean Energy Infrastructure Bond channels verified institutional financing into certified sustainable solar grids and wind harvesting infrastructure. Offers an enhanced yield of 13.20% p.a. while supporting carbon offset initiatives.",
            MaturityTerms = "Maturity occurs 180 days from issuance. Interest is calculated based on project yield distributions and paid in full along with the principal at term maturity.",
            PayoutFrequency = "At Maturity",
            IconName = "eco",
            Highlights =
            [
                "High return rate (13.20% p.a.)",
                "ESG certified renewable development projects",
                "Medium volatility rating with credit enhancement",
                "6-month investment horizon"
            ]
        },
        new InvestmentProduct
        {
            Id = "balanced-wealth-portfolio-365d",
            Name = "Balanced Wealth Multi-Asset Portfolio",
            Category = "Multi-Asset",
            RiskLevel = RiskLevel.Medium,
            AnnualInterestRate = 16.50m,
            DurationDays = 365,
            DurationLabel = "365 Days (1 Year)",
            MinimumAmount = 1_000.00m,
            MaximumAmount = 2_000_000.00m,
            Currency = "USD",
            CurrencySymbol = "$",
            ShortDescription = "Balanced diversification across corporate bonds and dividend blue-chip assets.",
            FullDescription = "Designed for intermediate to long-term wealth builders, the Balanced Wealth Multi-Asset Portfolio diversifies across high-grade corporate bonds, real estate trust notes, and blue-chip equities to maximize yield stability across market cycles.",
            MaturityTerms = "12-month tenure with comprehensive annual yield distribution. Funds become fully liquid on maturity date with optional auto-reinvestment into new cycles.",
            PayoutFrequency = "At Maturity",
            IconName = "pie_chart",
            Highlights =
            [
                "Comprehensive asset diversification",
                "Targeted 16.50% annualized performance",
                "Managed risk mitigation protocol",
                "Suitable for long-term compounding"
            ]
        },
        new InvestmentProduct
        {
            Id = "tech-innovators-equity-180d",
            Name = "Tech Innovators Alpha Fund",
            Category = "Equity Fund",
            RiskLevel = RiskLevel.High,
            AnnualInterestRate = 21.50m,
            DurationDays = 180,
            DurationLabel = "180 Days (6 Months)",
            MinimumAmount = 2_500.00m,
            MaximumAmount = 5_000_000.00m,
            Currency = "USD",
            CurrencySymbol = "$",
            ShortDescription = "Aggressive alpha fund focused on next-generation artificial intelligence and cloud leaders.",
            FullDescription = "An aggressive growth strategy targeting high-momentum market leaders in artificial intelligence, semiconductor innovation, and next-generation cloud infrastructure. Offers superior return potential for higher-risk tolerance investors.",
            MaturityTerms = "180-day lockup cycle with variable performance tracking. Target return benchmarked at 21.50% p.a. Subject to market movement conditions.",
            PayoutFrequency = "At Maturity",
            IconName = "trending_up",
            Highlights =
            [
                "High-alpha growth potential (21.50% p.a.)",
                "Direct exposure to top AI & semiconductor pioneers",
                "Professional portfolio rebalancing",
                "Designed for experienced growth investors"
            ]
        }
    ];

    public Task<IReadOnlyList<InvestmentProduct>> GetAllProductsAsync(
        string? category = null, 
        RiskLevel? riskLevel = null, 
        string? search = null)
    {
        IEnumerable<InvestmentProduct> query = SeedProducts.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (riskLevel.HasValue)
        {
            query = query.Where(p => p.RiskLevel == riskLevel.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => 
                p.Id.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.ShortDescription.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.FullDescription.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                p.Highlights.Any(h => h.Contains(s, StringComparison.OrdinalIgnoreCase)));
        }

        return Task.FromResult<IReadOnlyList<InvestmentProduct>>(query.ToList());
    }

    public Task<InvestmentProduct?> GetProductByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<InvestmentProduct?>(null);

        var product = SeedProducts.FirstOrDefault(p => 
            p.Id.Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(product);
    }
}
