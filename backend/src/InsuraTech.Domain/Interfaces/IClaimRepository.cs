namespace InsuraTech.Domain.Interfaces;

using InsuraTech.Domain.Claims;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

public interface IClaimRepository
{
    Task<DomainClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<DomainClaim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DomainClaim>> GetAllAsync(ClaimStatus? status, Guid? policyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ClaimStatus? status, Guid? policyId, CancellationToken cancellationToken = default);
    Task<int> CountOpenClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task AddAsync(DomainClaim claim, CancellationToken cancellationToken = default);
    Task UpdateAsync(DomainClaim claim, CancellationToken cancellationToken = default);
}