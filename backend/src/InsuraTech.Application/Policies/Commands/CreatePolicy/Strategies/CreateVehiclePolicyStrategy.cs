using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;

public sealed class CreateVehiclePolicyStrategy : ICreatePolicyStrategy
{
    public PolicyType Type => PolicyType.Vehicle;

    public bool CanHandle(CreatePolicyCommand request) =>
        !string.IsNullOrWhiteSpace(request.VehiclePlanId)
        && request.VehicleCommercialValue.HasValue
        && request.VehicleYear.HasValue
        && !string.IsNullOrWhiteSpace(request.VehicleBrand);

    public Task<Policy> CreateAsync(CreatePolicyCommand request, PolicyNumber policyNumber, InsuredPerson insured, CancellationToken cancellationToken)
    {
        var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
        var policy   = Policy.CreateVehiclePolicy(
            policyNumber, insured, coverage,
            request.VehiclePlanId!,
            request.VehicleCommercialValue!.Value,
            request.VehicleYear!.Value,
            request.VehicleBrand!,
            DateOnly.FromDateTime(DateTime.UtcNow));

        return Task.FromResult(policy);
    }
}
