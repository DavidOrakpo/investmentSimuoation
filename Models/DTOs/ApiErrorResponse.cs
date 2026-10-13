namespace investmentSimuoation.Models.DTOs;

/// <summary>
/// Structured error response model for clear client-side error handling.
/// </summary>
public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public ApiErrorResponse() { }

    public ApiErrorResponse(int statusCode, string message, string? errorCode = null)
    {
        StatusCode = statusCode;
        Message = message;
        ErrorCode = errorCode;
    }
}
