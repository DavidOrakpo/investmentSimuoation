using investmentSimuoation.Models.Domain;

namespace investmentSimuoation.Services;

public interface IProductService
{
    Task<IReadOnlyList<InvestmentProduct>> GetAllProductsAsync(
        string? category = null, 
        RiskLevel? riskLevel = null, 
        string? search = null);

    Task<InvestmentProduct?> GetProductByIdAsync(string id);
}
