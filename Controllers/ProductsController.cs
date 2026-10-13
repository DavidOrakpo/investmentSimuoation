using investmentSimuoation.Models.Domain;
using investmentSimuoation.Models.DTOs;
using investmentSimuoation.Services;
using Microsoft.AspNetCore.Mvc;

namespace investmentSimuoation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of available investment products.
    /// Supports test parameters for simulating empty, error, and loading states in the application.
    /// </summary>
    /// <param name="category">Optional category filter (e.g. Government Bonds, Fixed Income).</param>
    /// <param name="riskLevel">Optional risk level filter (Low, Medium, High).</param>
    /// <param name="search">Optional search keyword matching name, category, or description.</param>
    /// <param name="simulateEmpty">If true, returns an empty list to test empty state UI.</param>
    /// <param name="simulateError">If true, returns a 500 error to test error & retry UI.</param>
    /// <param name="simulateDelayMs">Optional delay in milliseconds to test loading spinners/shimmers.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category = null,
        [FromQuery] RiskLevel? riskLevel = null,
        [FromQuery] string? search = null,
        [FromQuery] bool simulateEmpty = false,
        [FromQuery] bool simulateError = false,
        [FromQuery] int? simulateDelayMs = null)
    {
        if (simulateDelayMs.HasValue && simulateDelayMs.Value > 0)
        {
            var delay = Math.Min(simulateDelayMs.Value, 10000);
            await Task.Delay(delay);
        }

        if (simulateError)
        {
            _logger.LogWarning("Simulation error triggered on GET /api/products.");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = "SIMULATED_ERROR",
                Message = "Simulated server failure for error state and retry testing."
            });
        }

        if (simulateEmpty)
        {
            return Ok(Array.Empty<ProductDto>());
        }

        var products = await _productService.GetAllProductsAsync(category, riskLevel, search);
        var dtos = products.Select(ProductDto.FromDomain).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves full details for a specific investment product by its ID.
    /// </summary>
    /// <param name="id">Product identifier (e.g. treasury-yield-30d).</param>
    /// <param name="simulateDelayMs">Optional delay in milliseconds to test loading states.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        string id, 
        [FromQuery] int? simulateDelayMs = null)
    {
        if (simulateDelayMs.HasValue && simulateDelayMs.Value > 0)
        {
            var delay = Math.Min(simulateDelayMs.Value, 10000);
            await Task.Delay(delay);
        }

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                ErrorCode = "PRODUCT_NOT_FOUND",
                Message = $"Investment product with ID '{id}' was not found."
            });
        }

        return Ok(ProductDto.FromDomain(product));
    }
}
