using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;

public sealed class CreateHealthPolicyStrategy : ICreatePolicyStrategy
{
    public PolicyType Type => PolicyType.Health;

    public bool CanHandle(CreatePolicyCommand request) =>
        !string.IsNullOrWhiteSpace(request.HealthPlanId);

    public Task<Policy> CreateAsync(CreatePolicyCommand request, PolicyNumber policyNumber, InsuredPerson insured, CancellationToken cancellationToken)
    {
        var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
        var policy   = Policy.CreateHealthPolicy(
            policyNumber, insured, coverage,
            request.MonthlyPremium, request.HealthPlanId!,
            DateOnly.FromDateTime(DateTime.UtcNow));

        return Task.FromResult(policy);
    }
}
