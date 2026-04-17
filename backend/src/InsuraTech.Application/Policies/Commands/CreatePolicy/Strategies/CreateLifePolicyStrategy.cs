using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;

public sealed class CreateLifePolicyStrategy : ICreatePolicyStrategy
{
    public PolicyType Type => PolicyType.Life;

    public bool CanHandle(CreatePolicyCommand request) =>
        !string.IsNullOrWhiteSpace(request.LifePlanId);

    public Task<Policy> CreateAsync(CreatePolicyCommand request, PolicyNumber policyNumber, InsuredPerson insured, CancellationToken cancellationToken)
    {
        var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
        var policy   = Policy.CreateLifePolicy(
            policyNumber, insured, coverage,
            request.LifePlanId!,
            DateOnly.FromDateTime(DateTime.UtcNow));

        return Task.FromResult(policy);
    }
}
