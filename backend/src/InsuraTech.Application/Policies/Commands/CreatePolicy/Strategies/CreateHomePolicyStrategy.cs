using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.HomePlan;
using InsuraTech.Domain.Policies.ValueObjects;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;

public sealed class CreateHomePolicyStrategy : ICreatePolicyStrategy
{
    public PolicyType Type => PolicyType.Home;

    public bool CanHandle(CreatePolicyCommand request) =>
        request.HomePropertyValue.HasValue
        && request.HomeConstructionYear.HasValue
        && request.HomeStratum.HasValue
        && request.HomeOccupants.HasValue
        && !string.IsNullOrWhiteSpace(request.HomePropertyType)
        && request.HomeSelectedCoverages?.Count > 0;

    public Task<Policy> CreateAsync(CreatePolicyCommand request, PolicyNumber policyNumber, InsuredPerson insured, CancellationToken cancellationToken)
    {
        var coverage     = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
        var propertyType = Enum.Parse<HomePropertyType>(request.HomePropertyType!, ignoreCase: true);
        var coverages    = request.HomeSelectedCoverages!
            .Select(c => Enum.Parse<HomeCoverage>(c, ignoreCase: true))
            .ToList()
            .AsReadOnly();

        var policy = Policy.CreateHomePolicy(
            policyNumber, insured, coverage,
            request.HomePlanPackageId,
            request.HomePropertyValue!.Value,
            request.HomeConstructionYear!.Value,
            request.HomeStratum!.Value,
            request.HomeOccupants!.Value,
            propertyType, coverages,
            DateOnly.FromDateTime(DateTime.UtcNow));

        return Task.FromResult(policy);
    }
}
