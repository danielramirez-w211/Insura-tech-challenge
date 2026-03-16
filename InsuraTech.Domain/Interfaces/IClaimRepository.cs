using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace InsuraTech.Domain.Interfaces
{
    public interface IClaimRepository
    {
        Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default);
        Task<int> CountOpenClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default);
        Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
        Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default);
    }
}
