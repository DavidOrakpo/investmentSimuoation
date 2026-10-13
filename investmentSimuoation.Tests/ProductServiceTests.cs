using investmentSimuoation.Models.Domain;
using investmentSimuoation.Services;

namespace investmentSimuoation.Tests;

public class ProductServiceTests
{
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productService = new ProductService();
    }

    [Fact]
    public async Task GetAllProducts_ReturnsAtLeastThreeSampleProducts()
    {
        // Act
        var products = await _productService.GetAllProductsAsync();

        // Assert
        Assert.NotNull(products);
        Assert.True(products.Count >= 3, "Requirements require at least 3 sample products.");
        Assert.All(products, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.True(p.AnnualInterestRate > 0);
            Assert.True(p.MinimumAmount > 0);
            Assert.True(p.DurationDays > 0);
            Assert.False(string.IsNullOrWhiteSpace(p.MaturityTerms));
        });
    }

    [Fact]
    public async Task GetProductById_WhenExists_ReturnsProduct()
    {
        // Act
        var product = await _productService.GetProductByIdAsync("treasury-yield-30d");

        // Assert
        Assert.NotNull(product);
        Assert.Equal("Federal Treasury 30-Day Note", product.Name);
        Assert.Equal(30, product.DurationDays);
        Assert.Equal(7.50m, product.AnnualInterestRate);
        Assert.Equal(100.00m, product.MinimumAmount);
    }

    [Fact]
    public async Task GetProductById_WhenDoesNotExist_ReturnsNull()
    {
        // Act
        var product = await _productService.GetProductByIdAsync("invalid-product-id");

        // Assert
        Assert.Null(product);
    }

    [Fact]
    public async Task GetAllProducts_WhenFilteredByCategory_ReturnsMatchingProductsOnly()
    {
        // Act
        var products = await _productService.GetAllProductsAsync(category: "Government Bonds");

        // Assert
        Assert.NotEmpty(products);
        Assert.All(products, p => Assert.Equal("Government Bonds", p.Category, ignoreCase: true));
    }

    [Fact]
    public async Task GetAllProducts_WhenFilteredByRisk_ReturnsMatchingProductsOnly()
    {
        // Act
        var products = await _productService.GetAllProductsAsync(riskLevel: RiskLevel.High);

        // Assert
        Assert.NotEmpty(products);
        Assert.All(products, p => Assert.Equal(RiskLevel.High, p.RiskLevel));
    }

    [Fact]
    public async Task GetAllProducts_WhenFilteredBySearch_ReturnsMatchingProducts()
    {
        // Act
        var products = await _productService.GetAllProductsAsync(search: "green");

        // Assert
        Assert.NotEmpty(products);
        Assert.Contains(products, p => p.Name.Contains("Clean Energy", StringComparison.OrdinalIgnoreCase) || 
                                       p.ShortDescription.Contains("solar", StringComparison.OrdinalIgnoreCase));
    }
}
