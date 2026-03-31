using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Claims;
using DomainClaim = InsuraTech.Domain.Claims.Claim;
using Microsoft.EntityFrameworkCore;


namespace InsuraTech.Infrastructure.Persistence.Repositories
{
    public sealed class ClaimRepository : IClaimRepository
    {
        private readonly InsuraTechDbContext _context;

        public ClaimRepository(InsuraTechDbContext context)
        {
            _context = context;
        }

        public async Task<DomainClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Claims
                .Include(c => c.StatusHistory)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DomainClaim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return await _context.Claims
                .Include(c => c.StatusHistory)
                .Where(c => c.PolicyId == policyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountOpenClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
        {
            return await _context.Claims
                .CountAsync(c => c.PolicyId == policyId && (
                    c.Status == ClaimStatus.Registered ||
                    c.Status == ClaimStatus.UnderInvestigation ||
                    c.Status == ClaimStatus.Appealed),
                cancellationToken);
        }

        public async Task AddAsync(DomainClaim claim, CancellationToken cancellationToken = default)
        {
            await _context.Claims.AddAsync(claim, cancellationToken);
        }

        public async Task UpdateAsync(DomainClaim claim, CancellationToken cancellationToken = default)
        {
            _context.Claims.Update(claim);
            await Task.CompletedTask;
        }
    }
}
