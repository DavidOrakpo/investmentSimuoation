using investmentSimuoation.Models.Domain;
using investmentSimuoation.Models.DTOs;

namespace investmentSimuoation.Services;

public interface IInvestmentService
{
    Task<InvestmentSimulationResultDto> SimulateInvestmentAsync(SimulateInvestmentRequest request);

    Task<(Investment Investment, bool WasDuplicatePrevented)> CreateInvestmentAsync(
        CreateInvestmentRequest request, 
        string? headerIdempotencyKey = null);

    Task<IReadOnlyList<Investment>> GetAllInvestmentsAsync();

    Task<Investment?> GetInvestmentByReferenceAsync(string referenceNumber);

    Task ResetInvestmentsAsync();
}
