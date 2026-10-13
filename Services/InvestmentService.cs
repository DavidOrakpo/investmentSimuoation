using System.Collections.Concurrent;
using investmentSimuoation.Models.Domain;
using investmentSimuoation.Models.DTOs;
using investmentSimuoation.Models.Exceptions;

namespace investmentSimuoation.Services;

public class InvestmentService : IInvestmentService
{
    private readonly IProductService _productService;
    private readonly ConcurrentDictionary<string, Investment> _investments = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _idempotencyMap = new(StringComparer.OrdinalIgnoreCase);

    public InvestmentService(IProductService productService)
    {
        _productService = productService;
        SeedInitialInvestments();
    }

    public async Task<InvestmentSimulationResultDto> SimulateInvestmentAsync(SimulateInvestmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await _productService.GetProductByIdAsync(request.ProductId)
            ?? throw new ProductNotFoundException(request.ProductId);

        ValidateInvestmentAmount(request.Amount, product);

        var calculation = CalculateProjection(request.Amount, product.AnnualInterestRate, product.DurationDays);
        var now = DateTime.UtcNow;
        var maturityDate = now.AddDays(product.DurationDays);

        return new InvestmentSimulationResultDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Category = product.Category,
            RiskLevel = product.RiskLevel.ToString(),
            InvestedAmount = request.Amount,
            AnnualInterestRate = product.AnnualInterestRate,
            ReturnRateDisplay = $"{product.AnnualInterestRate:0.##}% p.a.",
            DurationDays = product.DurationDays,
            DurationLabel = product.DurationLabel,
            ExpectedReturnAmount = calculation.ExpectedInterest,
            TotalPayoutAtMaturity = calculation.TotalPayout,
            StartDateUtc = now,
            MaturityDateUtc = maturityDate,
            FormattedMaturityDate = maturityDate.ToString("MMM dd, yyyy"),
            Currency = product.Currency,
            CurrencySymbol = product.CurrencySymbol,
            MaturityTerms = product.MaturityTerms,
            PayoutFrequency = product.PayoutFrequency,
            SummaryBreakdown = new InvestmentSummaryBreakdown
            {
                Principal = request.Amount,
                EstimatedEarnings = calculation.ExpectedInterest,
                ManagementFee = 0.00m,
                NetEstimatedPayout = calculation.TotalPayout
            }
        };
    }

    public async Task<(Investment Investment, bool WasDuplicatePrevented)> CreateInvestmentAsync(
        CreateInvestmentRequest request, 
        string? headerIdempotencyKey = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Normalize idempotency key (header takes precedence over body if provided)
        var idempotencyKey = !string.IsNullOrWhiteSpace(headerIdempotencyKey) 
            ? headerIdempotencyKey.Trim() 
            : request.IdempotencyKey?.Trim();

        // Check if an investment was already processed for this idempotency key
        if (!string.IsNullOrWhiteSpace(idempotencyKey) && 
            _idempotencyMap.TryGetValue(idempotencyKey, out var existingRef))
        {
            if (_investments.TryGetValue(existingRef, out var existingInvestment))
            {
                return (existingInvestment, true);
            }
        }

        var product = await _productService.GetProductByIdAsync(request.ProductId)
            ?? throw new ProductNotFoundException(request.ProductId);

        ValidateInvestmentAmount(request.Amount, product);

        var calculation = CalculateProjection(request.Amount, product.AnnualInterestRate, product.DurationDays);
        var now = DateTime.UtcNow;
        var maturityDate = now.AddDays(product.DurationDays);
        var referenceNumber = GenerateReferenceNumber();

        var investment = new Investment
        {
            ReferenceNumber = referenceNumber,
            ProductId = product.Id,
            ProductName = product.Name,
            Category = product.Category,
            RiskLevel = product.RiskLevel,
            InvestedAmount = request.Amount,
            AnnualInterestRate = product.AnnualInterestRate,
            DurationDays = product.DurationDays,
            DurationLabel = product.DurationLabel,
            ExpectedReturnAmount = calculation.ExpectedInterest,
            TotalPayoutAtMaturity = calculation.TotalPayout,
            Currency = product.Currency,
            CurrencySymbol = product.CurrencySymbol,
            Status = "Active",
            InvestorName = string.IsNullOrWhiteSpace(request.InvestorName) ? "Demo Investor" : request.InvestorName.Trim(),
            FundingSource = string.IsNullOrWhiteSpace(request.FundingSource) ? "Primary Wallet" : request.FundingSource.Trim(),
            IdempotencyKey = idempotencyKey,
            CreatedAtUtc = now,
            MaturityDateUtc = maturityDate
        };

        _investments[referenceNumber] = investment;

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            _idempotencyMap[idempotencyKey] = referenceNumber;
        }

        return (investment, false);
    }

    public Task<IReadOnlyList<Investment>> GetAllInvestmentsAsync()
    {
        var list = _investments.Values
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToList();

        return Task.FromResult<IReadOnlyList<Investment>>(list);
    }

    public Task<Investment?> GetInvestmentByReferenceAsync(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
            return Task.FromResult<Investment?>(null);

        _investments.TryGetValue(referenceNumber.Trim(), out var investment);
        return Task.FromResult(investment);
    }

    public Task ResetInvestmentsAsync()
    {
        _investments.Clear();
        _idempotencyMap.Clear();
        SeedInitialInvestments();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure financial calculation for simple interest on duration tenure.
    /// Expected Interest = Amount * (AnnualRate / 100) * (DurationDays / 365)
    /// </summary>
    public static (decimal ExpectedInterest, decimal TotalPayout) CalculateProjection(
        decimal amount, 
        decimal annualRatePercentage, 
        int durationDays)
    {
        if (amount <= 0 || durationDays <= 0 || annualRatePercentage < 0)
            return (0m, amount);

        var rateDecimal = annualRatePercentage / 100m;
        var tenureFraction = (decimal)durationDays / 365m;
        var interest = Math.Round(amount * rateDecimal * tenureFraction, 2, MidpointRounding.AwayFromZero);
        var total = amount + interest;

        return (interest, total);
    }

    private static void ValidateInvestmentAmount(decimal amount, InvestmentProduct product)
    {
        if (amount <= 0)
        {
            throw new InvestmentValidationException(
                "amount", 
                "Investment amount must be greater than zero.", 
                attemptedAmount: amount);
        }

        if (amount < product.MinimumAmount)
        {
            throw new InvestmentValidationException(
                "amount", 
                $"Investment amount {product.CurrencySymbol}{amount:N2} is below the required minimum of {product.CurrencySymbol}{product.MinimumAmount:N2} for {product.Name}.",
                attemptedAmount: amount,
                requiredMinimum: product.MinimumAmount);
        }

        if (product.MaximumAmount.HasValue && amount > product.MaximumAmount.Value)
        {
            throw new InvestmentValidationException(
                "amount", 
                $"Investment amount {product.CurrencySymbol}{amount:N2} exceeds the maximum allowed limit of {product.CurrencySymbol}{product.MaximumAmount.Value:N2}.",
                attemptedAmount: amount);
        }
    }

    private static string GenerateReferenceNumber()
    {
        var randomHex = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{randomHex}";
    }

    private void SeedInitialInvestments()
    {
        var sample1 = new Investment
        {
            ReferenceNumber = "INV-SAMPLE-001",
            ProductId = "treasury-yield-30d",
            ProductName = "Federal Treasury 30-Day Note",
            Category = "Government Bonds",
            RiskLevel = RiskLevel.Low,
            InvestedAmount = 500.00m,
            AnnualInterestRate = 7.50m,
            DurationDays = 30,
            DurationLabel = "30 Days",
            ExpectedReturnAmount = 3.08m,
            TotalPayoutAtMaturity = 503.08m,
            Currency = "USD",
            CurrencySymbol = "$",
            Status = "Active",
            InvestorName = "Demo Investor",
            FundingSource = "Primary Wallet",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-5),
            MaturityDateUtc = DateTime.UtcNow.AddDays(25)
        };

        _investments[sample1.ReferenceNumber] = sample1;
    }
}
