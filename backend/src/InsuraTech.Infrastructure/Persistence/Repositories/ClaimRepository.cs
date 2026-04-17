using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Infrastructure.Persistence.Repositories.Base;
using MongoDB.Driver;
using DomainClaim = InsuraTech.Domain.Claims.Claim;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class ClaimRepository : MongoRepository<DomainClaim>, IClaimRepository
{
    public ClaimRepository(MongoDbContext context) : base(context) { }

    public async Task<DomainClaim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Claims
            .Find(c => c.Id == id && !c.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainClaim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return await Context.Claims
            .Find(c => c.PolicyId == policyId && !c.IsDeleted)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainClaim>> GetAllAsync(
        ClaimStatus? status, Guid? policyId, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, policyId);
        return await ApplyPagination(
                Context.Claims.Find(filter).SortByDescending(c => c.CreatedAt),
                page, pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(ClaimStatus? status, Guid? policyId, CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, policyId);
        return (int)await Context.Claims.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<int> CountOpenClaimsByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var openStatuses = new[] { ClaimStatus.Registered, ClaimStatus.UnderInvestigation, ClaimStatus.Appealed };

        var filter = Builders<DomainClaim>.Filter.And(
            Builders<DomainClaim>.Filter.Eq(c => c.PolicyId, policyId),
            Builders<DomainClaim>.Filter.In(c => c.Status, openStatuses),
            NotDeleted());

        return (int)await Context.Claims.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task AddAsync(DomainClaim claim, CancellationToken cancellationToken = default)
    {
        await Context.Claims.InsertOneAsync(claim, cancellationToken: cancellationToken);
        await Context.PublishDomainEventsAsync(claim, cancellationToken);
    }

    public async Task UpdateAsync(DomainClaim claim, CancellationToken cancellationToken = default)
    {
        await Context.Claims.ReplaceOneAsync(
            c => c.Id == claim.Id,
            claim,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);

        await Context.PublishDomainEventsAsync(claim, cancellationToken);
    }

    private static FilterDefinition<DomainClaim> BuildFilter(ClaimStatus? status, Guid? policyId)
    {
        var filters = new List<FilterDefinition<DomainClaim>> { NotDeleted() };

        if (status.HasValue)
            filters.Add(Builders<DomainClaim>.Filter.Eq(c => c.Status, status.Value));

        if (policyId.HasValue)
            filters.Add(Builders<DomainClaim>.Filter.Eq(c => c.PolicyId, policyId.Value));

        return Builders<DomainClaim>.Filter.And(filters);
    }
}
