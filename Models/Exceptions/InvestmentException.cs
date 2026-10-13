namespace investmentSimuoation.Models.Exceptions;

/// <summary>
/// Thrown when an investment product is not found.
/// </summary>
public class ProductNotFoundException : Exception
{
    public string ProductId { get; }

    public ProductNotFoundException(string productId) 
        : base($"Investment product with ID '{productId}' was not found.")
    {
        ProductId = productId;
    }
}

/// <summary>
/// Thrown when investment validation rules fail (e.g. below minimum amount).
/// </summary>
public class InvestmentValidationException : Exception
{
    public string FieldName { get; }
    public decimal? AttemptedAmount { get; }
    public decimal? RequiredMinimum { get; }

    public InvestmentValidationException(string fieldName, string message, decimal? attemptedAmount = null, decimal? requiredMinimum = null) 
        : base(message)
    {
        FieldName = fieldName;
        AttemptedAmount = attemptedAmount;
        RequiredMinimum = requiredMinimum;
    }
}
