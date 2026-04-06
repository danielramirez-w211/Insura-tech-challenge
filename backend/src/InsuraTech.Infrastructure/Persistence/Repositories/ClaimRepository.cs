using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Interfaces;
using MongoDB.Driver;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly MongoDbContext _context;

    public ClaimRepository(MongoDbContext context) => _context = context;

    public async Task<DomainClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Find(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainClaim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Find(c => c.PolicyId == policyId && !c.IsDeleted)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainClaim>> GetAllAsync(
        ClaimStatus? status, Guid? policyId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, policyId);
        return await _context.Claims
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ClaimStatus? status, Guid? policyId, CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, policyId);
        return (int)await _context.Claims.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<int> CountOpenClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var openStatuses = new[] { ClaimStatus.Registered, ClaimStatus.UnderInvestigation, ClaimStatus.Appealed };

        var filter = Builders<DomainClaim>.Filter.And(
            Builders<DomainClaim>.Filter.Eq(c => c.PolicyId, policyId),
            Builders<DomainClaim>.Filter.In(c => c.Status, openStatuses),
            Builders<DomainClaim>.Filter.Eq(c => c.IsDeleted, false));

        return (int)await _context.Claims.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    private static FilterDefinition<DomainClaim> BuildFilter(ClaimStatus? status, Guid? policyId)
    {
        var filters = new List<FilterDefinition<DomainClaim>>
        {
            Builders<DomainClaim>.Filter.Eq(c => c.IsDeleted, false)
        };
        if (status.HasValue)
            filters.Add(Builders<DomainClaim>.Filter.Eq(c => c.Status, status.Value));
        if (policyId.HasValue)
            filters.Add(Builders<DomainClaim>.Filter.Eq(c => c.PolicyId, policyId.Value));
        return Builders<DomainClaim>.Filter.And(filters);
    }

    public async Task AddAsync(DomainClaim claim, CancellationToken cancellationToken = default)
    {
        await _context.Claims.InsertOneAsync(claim, cancellationToken: cancellationToken);
        await _context.PublishDomainEventsAsync(claim, cancellationToken);
    }

    public async Task UpdateAsync(DomainClaim claim, CancellationToken cancellationToken = default)
    {
        await _context.Claims.ReplaceOneAsync(
            c => c.Id == claim.Id,
            claim,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);

        await _context.PublishDomainEventsAsync(claim, cancellationToken);
    }
}
