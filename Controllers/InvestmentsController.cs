using investmentSimuoation.Models.DTOs;
using investmentSimuoation.Models.Exceptions;
using investmentSimuoation.Services;
using Microsoft.AspNetCore.Mvc;

namespace investmentSimuoation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InvestmentsController : ControllerBase
{
    private readonly IInvestmentService _investmentService;
    private readonly ILogger<InvestmentsController> _logger;

    public InvestmentsController(IInvestmentService investmentService, ILogger<InvestmentsController> logger)
    {
        _investmentService = investmentService;
        _logger = logger;
    }

    /// <summary>
    /// Simulates / previews an investment calculation without creating a persistent record.
    /// </summary>
    [HttpPost("simulate")]
    [HttpPost("preview")]
    [ProducesResponseType(typeof(InvestmentSimulationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SimulateInvestment(
        [FromBody] SimulateInvestmentRequest request,
        [FromQuery] int? simulateDelayMs = null,
        [FromQuery] bool simulateError = false)
    {
        if (simulateDelayMs.HasValue && simulateDelayMs.Value > 0)
        {
            await Task.Delay(Math.Min(simulateDelayMs.Value, 10000));
        }

        if (simulateError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = "SIMULATED_ERROR",
                Message = "Simulated calculation failure for testing."
            });
        }

        try
        {
            var result = await _investmentService.SimulateInvestmentAsync(request);
            return Ok(result);
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse(StatusCodes.Status404NotFound, ex.Message, "PRODUCT_NOT_FOUND"));
        }
        catch (InvestmentValidationException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = "VALIDATION_FAILED",
                Message = ex.Message,
                ValidationErrors = new Dictionary<string, string[]>
                {
                    [ex.FieldName] = [ex.Message]
                }
            });
        }
    }

    /// <summary>
    /// Creates and simulates a new investment, returning a unique reference number.
    /// Includes built-in duplicate prevention via idempotency keys passed in the 'Idempotency-Key' header or request body.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvestmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(InvestmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInvestment(
        [FromBody] CreateInvestmentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null,
        [FromQuery] int? simulateDelayMs = null,
        [FromQuery] bool simulateError = false)
    {
        if (simulateDelayMs.HasValue && simulateDelayMs.Value > 0)
        {
            await Task.Delay(Math.Min(simulateDelayMs.Value, 10000));
        }

        if (simulateError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = "SIMULATED_ERROR",
                Message = "Simulated investment creation failure for testing error handling."
            });
        }

        try
        {
            var (investment, wasDuplicatePrevented) = await _investmentService.CreateInvestmentAsync(request, idempotencyKey);
            var response = InvestmentResponseDto.FromDomain(investment, wasDuplicatePrevented);

            if (wasDuplicatePrevented)
            {
                _logger.LogInformation("Duplicate submission prevented for key {Key}. Returning reference {Ref}.", 
                    investment.IdempotencyKey, investment.ReferenceNumber);
                return Ok(response);
            }

            return CreatedAtAction(
                nameof(GetByReference), 
                new { referenceNumber = investment.ReferenceNumber }, 
                response);
        }
        catch (ProductNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse(StatusCodes.Status404NotFound, ex.Message, "PRODUCT_NOT_FOUND"));
        }
        catch (InvestmentValidationException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = "VALIDATION_FAILED",
                Message = ex.Message,
                ValidationErrors = new Dictionary<string, string[]>
                {
                    [ex.FieldName] = [ex.Message]
                }
            });
        }
    }

    /// <summary>
    /// Retrieves all created investments.
    /// Useful for showing portfolio / investment history in the application.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InvestmentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool simulateEmpty = false,
        [FromQuery] bool simulateError = false,
        [FromQuery] int? simulateDelayMs = null)
    {
        if (simulateDelayMs.HasValue && simulateDelayMs.Value > 0)
        {
            await Task.Delay(Math.Min(simulateDelayMs.Value, 10000));
        }

        if (simulateError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorCode = "SIMULATED_ERROR",
                Message = "Simulated failure for portfolio history testing."
            });
        }

        if (simulateEmpty)
        {
            return Ok(Array.Empty<InvestmentResponseDto>());
        }

        var investments = await _investmentService.GetAllInvestmentsAsync();
        var dtos = investments.Select(i => InvestmentResponseDto.FromDomain(i)).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Retrieves details of an investment by its unique reference number.
    /// </summary>
    [HttpGet("{referenceNumber}")]
    [ProducesResponseType(typeof(InvestmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByReference(string referenceNumber)
    {
        var investment = await _investmentService.GetInvestmentByReferenceAsync(referenceNumber);
        if (investment == null)
        {
            return NotFound(new ApiErrorResponse(
                StatusCodes.Status404NotFound, 
                $"Investment with reference number '{referenceNumber}' was not found.", 
                "INVESTMENT_NOT_FOUND"));
        }

        return Ok(InvestmentResponseDto.FromDomain(investment));
    }

    /// <summary>
    /// Resets all simulated investments to initial seeded state (helpful for testing & test runs).
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reset()
    {
        await _investmentService.ResetInvestmentsAsync();
        return Ok(new { message = "Investments reset to initial state successfully." });
    }
}
