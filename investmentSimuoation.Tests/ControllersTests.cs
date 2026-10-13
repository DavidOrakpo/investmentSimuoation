using investmentSimuoation.Controllers;
using investmentSimuoation.Models.Domain;
using investmentSimuoation.Models.DTOs;
using investmentSimuoation.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace investmentSimuoation.Tests;

public class ControllersTests
{
    private readonly ProductService _productService;
    private readonly InvestmentService _investmentService;
    private readonly ProductsController _productsController;
    private readonly InvestmentsController _investmentsController;

    public ControllersTests()
    {
        _productService = new ProductService();
        _investmentService = new InvestmentService(_productService);
        
        _productsController = new ProductsController(
            _productService, 
            NullLogger<ProductsController>.Instance);

        _investmentsController = new InvestmentsController(
            _investmentService, 
            NullLogger<InvestmentsController>.Instance);
    }

    [Fact]
    public async Task ProductsController_GetAll_ReturnsOkWithProducts()
    {
        // Act
        var result = await _productsController.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task ProductsController_GetAll_SimulateEmpty_ReturnsEmptyList()
    {
        // Act
        var result = await _productsController.GetAll(simulateEmpty: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Empty(products);
    }

    [Fact]
    public async Task ProductsController_GetAll_SimulateError_Returns500()
    {
        // Act
        var result = await _productsController.GetAll(simulateError: true);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        var error = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.Equal("SIMULATED_ERROR", error.ErrorCode);
    }

    [Fact]
    public async Task ProductsController_GetById_WhenNotFound_Returns404()
    {
        // Act
        var result = await _productsController.GetById("non-existent-product");

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var error = Assert.IsType<ApiErrorResponse>(notFound.Value);
        Assert.Equal("PRODUCT_NOT_FOUND", error.ErrorCode);
    }

    [Fact]
    public async Task InvestmentsController_SimulateInvestment_WhenValid_ReturnsCalculation()
    {
        // Arrange
        var request = new SimulateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 1000m
        };

        // Act
        var result = await _investmentsController.SimulateInvestment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var simulation = Assert.IsType<InvestmentSimulationResultDto>(okResult.Value);
        Assert.Equal(1000m, simulation.InvestedAmount);
        Assert.Equal(6.16m, simulation.ExpectedReturnAmount);
        Assert.Equal(1006.16m, simulation.TotalPayoutAtMaturity);
    }

    [Fact]
    public async Task InvestmentsController_SimulateInvestment_WhenAmountBelowMinimum_Returns400WithErrors()
    {
        // Arrange
        var request = new SimulateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 20m // Min is $100
        };

        // Act
        var result = await _investmentsController.SimulateInvestment(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var error = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
        Assert.Equal("VALIDATION_FAILED", error.ErrorCode);
        Assert.NotNull(error.ValidationErrors);
        Assert.True(error.ValidationErrors.ContainsKey("amount"));
    }

    [Fact]
    public async Task InvestmentsController_CreateInvestment_Success_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 500m,
            InvestorName = "Jane Doe"
        };

        // Act
        var result = await _investmentsController.CreateInvestment(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<InvestmentResponseDto>(createdResult.Value);
        Assert.StartsWith("INV-", response.ReferenceNumber);
        Assert.False(response.WasDuplicatePrevented);
    }

    [Fact]
    public async Task InvestmentsController_CreateInvestment_DuplicateHeader_Returns200WithDuplicateFlag()
    {
        // Arrange
        var idempotencyKey = $"idem-{Guid.NewGuid()}";
        var request = new CreateInvestmentRequest
        {
            ProductId = "treasury-yield-30d",
            Amount = 500m
        };

        // Act
        var firstResult = await _investmentsController.CreateInvestment(request, idempotencyKey: idempotencyKey);
        var secondResult = await _investmentsController.CreateInvestment(request, idempotencyKey: idempotencyKey);

        // Assert
        Assert.IsType<CreatedAtActionResult>(firstResult);
        var okResult = Assert.IsType<OkObjectResult>(secondResult);
        var secondDto = Assert.IsType<InvestmentResponseDto>(okResult.Value);
        Assert.True(secondDto.WasDuplicatePrevented);
    }
}
