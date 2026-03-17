using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace InsuraTech.Infrastructure.Persistence.Repositories
{
    public sealed class PolicyRepository : IPolicyRepository
    {
        private readonly InsuraTechDbContext _context;

        public PolicyRepository(InsuraTechDbContext context)
        {
            _context = context;
        }

        public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Policies
                .Include(p => p.StatusHistory)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Policy?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Policies
                .Include(p => p.StatusHistory)
                .FirstOrDefaultAsync(p => p.Number.Value == policyNumber, cancellationToken);
        }

        public async Task<Policy?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            return await _context.Policies
                .Include(p => p.StatusHistory)
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "IdempotencyKey") == idempotencyKey, cancellationToken);
        }

        public async Task<IEnumerable<Policy>> GetAllAsync(
            PolicyStatus? status,
            PolicyType? type,
            string? documentId,
            DateOnly? startDate,
            DateOnly? endDate,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Policies
                .Include(p => p.StatusHistory)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (type.HasValue)
                query = query.Where(p => p.Type == type.Value);

            if (!string.IsNullOrWhiteSpace(documentId))
                query = query.Where(p => p.Insured.DocumentId == documentId);

            if (startDate.HasValue)
                query = query.Where(p => p.Coverage.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Coverage.EndDate <= endDate.Value);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Policies.CountAsync(cancellationToken);
        }

        public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
        {
            await _context.Policies.AddAsync(policy, cancellationToken);
        }

        public async Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
        {
            _context.Policies.Update(policy);
            await Task.CompletedTask;
        }

        public async Task<long> GetNextSequenceAsync(CancellationToken cancellationToken = default)
        {
            var count = await _context.Policies
                .IgnoreQueryFilters()
                .CountAsync(cancellationToken);

            return count + 1;
        }
    }
}
