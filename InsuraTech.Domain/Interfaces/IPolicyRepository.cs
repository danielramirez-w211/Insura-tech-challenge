using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Policies;

namespace InsuraTech.Domain.Interfaces
{
    public interface IPolicyRepository
    {
        Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Policy?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Policy>> GetAllAsync(
        PolicyStatus? status,
        PolicyType? type,
        string? documentId,
        DateOnly? startDate,
        DateOnly? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
        Task<int> CountAsync(
            PolicyStatus? status,
            PolicyType? type,
            string? documentId,
            DateOnly? startDate,
            DateOnly? endDate,
            CancellationToken cancellationToken = default);
        Task AddAsync(Policy policy, CancellationToken cancellationToken = default);
        Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default);
        Task<long> GetNextSequenceAsync(CancellationToken cancellationToken = default);

        Task<Policy?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        void SetIdempotencyKey(Policy policy, string idempotencyKey);
    }
}
