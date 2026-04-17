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
            string? insuredSearch,
            string? insuredDocumentType,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<int> CountAsync(
            PolicyStatus? status,
            PolicyType? type,
            string? documentId,
            DateOnly? startDate,
            DateOnly? endDate,
            string? insuredSearch,
            string? insuredDocumentType,
            CancellationToken cancellationToken = default);
        Task AddAsync(Policy policy, CancellationToken cancellationToken = default);
        Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default);
        Task<long> GetNextSequenceAsync(CancellationToken cancellationToken = default);

        Task<Policy?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        void SetIdempotencyKey(Policy policy, string idempotencyKey);

        /// <summary>Cuenta pólizas creadas por un asesor específico (para salesCount del dashboard de líder).</summary>
        Task<int> CountByAdvisorIdAsync(Guid advisorId, CancellationToken cancellationToken = default);

        /// <summary>Devuelve clientes únicos (agrupados por documentId) de las pólizas del asesor.</summary>
        Task<IEnumerable<ClientSummaryProjection>> GetMyClientsAsync(Guid advisorId, CancellationToken cancellationToken = default);
    }

    public sealed record ClientSummaryProjection(
        string DocumentId,
        string DocumentType,
        string FirstName,
        string LastName,
        string CityName,
        int PolicyCount);
}
