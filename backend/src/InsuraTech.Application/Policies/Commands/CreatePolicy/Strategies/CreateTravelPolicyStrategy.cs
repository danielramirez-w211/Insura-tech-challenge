using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.TravelPlan;
using InsuraTech.Domain.Policies.ValueObjects;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;

public sealed class CreateTravelPolicyStrategy : ICreatePolicyStrategy
{
    private readonly ITrmService _trmService;

    public CreateTravelPolicyStrategy(ITrmService trmService) => _trmService = trmService;

    public PolicyType Type => PolicyType.Travel;

    public bool CanHandle(CreatePolicyCommand request) =>
        request.TripType.HasValue && request.DurationDays.HasValue;

    public async Task<Policy> CreateAsync(CreatePolicyCommand request, PolicyNumber policyNumber, InsuredPerson insured, CancellationToken cancellationToken)
    {
        // Para Travel se usa el constructor directo para evitar la validación de mínimo 30 días
        // (un viaje de 1-29 días es válido en Travel aunque no lo sea en otros productos)
        var coverage = new CoveragePeriod(request.CoverageStartDate, request.CoverageEndDate);

        decimal? trmValue = null;
        DateOnly? trmDate = null;

        if (request.TripType == TripType.Internacional)
        {
            var trm  = await _trmService.GetCurrentTrmAsync(cancellationToken);
            trmValue = trm.ValueCop;
            trmDate  = trm.Date;
        }

        return Policy.CreateTravelPolicy(
            policyNumber, insured, coverage,
            request.TripType!.Value, request.Continent,
            request.DurationDays!.Value, trmValue, trmDate);
    }
}
