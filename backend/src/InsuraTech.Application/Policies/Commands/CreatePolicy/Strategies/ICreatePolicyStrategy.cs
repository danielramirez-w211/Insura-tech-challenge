using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy.Strategies;

public interface ICreatePolicyStrategy
{
    PolicyType Type { get; }
    bool CanHandle(CreatePolicyCommand request);
    Task<Policy> CreateAsync(CreatePolicyCommand request, PolicyNumber policyNumber, InsuredPerson insured, CancellationToken cancellationToken);
}
