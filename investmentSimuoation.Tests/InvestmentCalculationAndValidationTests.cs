using investmentSimuoation.Models.DTOs;
using investmentSimuoation.Models.Exceptions;
using investmentSimuoation.Services;

namespace investmentSimuoation.Tests;

public class InvestmentCalculationAndValidationTests
{
    private readonly ProductService _productService;
    private readonly InvestmentService _investmentService;

    public InvestmentCalculationAndValidationTests()
    {
        _productService = new ProductService();
        _investmentService = new InvestmentService(_productService);
    }

    [Theory]
    [InlineData(1000.00, 7.50, 30, 6.16, 1006.16)]
    [InlineData(500.00, 9.80, 90, 12.08, 512.08)]
    [InlineData(2500.00, 13.20, 180, 162.74, 2662.74)]
    [InlineData(10000.00, 16.50, 365, 1650.00, 11650.00)]
    public void CalculateProjection_CalculatesAccurateInterestAndPayout(
        decimal amount, 
        decimal rate, 
        int durationDays, 
        decimal expectedInterest, 
        decimal expectedTotal)
    {
        // Act
        var (interest, total) = InvestmentService.CalculateProjection(amount, rate, durationDays);

        // Assert
        Assert.Equal(expectedInterest, interest);
        Assert.Equal(expectedTotal, total);
    }

    [Fact]
    public async Task SimulateInvestment_WhenProductNotFound_ThrowsProductNotFoundException()
    {
        // Arrange
        var request = new SimulateInvestmentRequest
        {
            ProductId = "non-existent-product-id",
            Amount = 500m
        };

        // Act & Assert
        await Assert.ThrowsAsync<ProductNotFoundException>(() => 
            _investmentService.SimulateInvestmentAsync(request));
    }

    [Fact]
    public async Task SimulateInvestment_WhenAmountBelowMinimum_ThrowsInvestmentValidationException()
    {
        // Arrange - treasury note has $100 minimum
        var request = new SimulateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 50.00m
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvestmentValidationException>(() => 
            _investmentService.SimulateInvestmentAsync(request));

        Assert.Equal("amount", ex.FieldName);
        Assert.Equal(50.00m, ex.AttemptedAmount);
        Assert.Equal(100.00m, ex.RequiredMinimum);
    }

    [Fact]
    public async Task SimulateInvestment_WhenValid_ReturnsAccurateProjectionsAndBreakdown()
    {
        // Arrange
        var request = new SimulateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 1000.00m
        };

        // Act
        var result = await _investmentService.SimulateInvestmentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("treasury-yield-30d", result.ProductId);
        Assert.Equal(1000.00m, result.InvestedAmount);
        Assert.Equal(6.16m, result.ExpectedReturnAmount);
        Assert.Equal(1006.16m, result.TotalPayoutAtMaturity);
        Assert.Equal(30, result.DurationDays);
        Assert.True(result.MaturityDateUtc > result.StartDateUtc);
        Assert.Equal(1000.00m, result.SummaryBreakdown.Principal);
        Assert.Equal(6.16m, result.SummaryBreakdown.EstimatedEarnings);
        Assert.Equal(1006.16m, result.SummaryBreakdown.NetEstimatedPayout);
    }

    [Fact]
    public async Task CreateInvestment_WhenValid_GeneratesUniqueReferenceNumber()
    {
        // Arrange
        var request = new CreateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 500.00m,
            InvestorName = "Alice Investor",
            FundingSource = "Main Checking Account"
        };

        // Act
        var (investment, wasDuplicatePrevented) = await _investmentService.CreateInvestmentAsync(request);

        // Assert
        Assert.NotNull(investment);
        Assert.False(wasDuplicatePrevented);
        Assert.StartsWith("INV-", investment.ReferenceNumber);
        Assert.Equal("Active", investment.Status);
        Assert.Equal(500.00m, investment.InvestedAmount);
        Assert.Equal("Alice Investor", investment.InvestorName);
        Assert.Equal("Main Checking Account", investment.FundingSource);
    }

    [Fact]
    public async Task CreateInvestment_WithSameIdempotencyKey_PreventsDuplicateSubmission()
    {
        // Arrange
        var idempotencyKey = $"test-idempotency-{Guid.NewGuid()}";
        var request1 = new CreateInvestmentRequest
        {
            ProductId = "fixed-deposit-90d",
            Amount = 500.00m,
            IdempotencyKey = idempotencyKey
        };

        var request2 = new CreateInvestmentRequest
        {
            ProductId = "fixed-deposit-90d",
            Amount = 500.00m,
            IdempotencyKey = idempotencyKey
        };

        // Act
        var (firstCall, firstDuplicate) = await _investmentService.CreateInvestmentAsync(request1);
        var (secondCall, secondDuplicate) = await _investmentService.CreateInvestmentAsync(request2);

        // Assert
        Assert.False(firstDuplicate);
        Assert.True(secondDuplicate);
        Assert.Equal(firstCall.ReferenceNumber, secondCall.ReferenceNumber);
        Assert.Equal(firstCall.InvestedAmount, secondCall.InvestedAmount);
    }

    [Fact]
    public async Task CreateInvestment_WhenAmountBelowMinimum_ThrowsValidationException()
    {
        // Arrange - "fixed-deposit-90d" minimum is $250.00
        var request = new CreateInvestmentRequest
        {
            ProductId = "fixed-deposit-90d",
            Amount = 100.00m
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvestmentValidationException>(() => 
            _investmentService.CreateInvestmentAsync(request));

        Assert.Equal("amount", ex.FieldName);
        Assert.Equal(100.00m, ex.AttemptedAmount);
        Assert.Equal(250.00m, ex.RequiredMinimum);
    }
}
