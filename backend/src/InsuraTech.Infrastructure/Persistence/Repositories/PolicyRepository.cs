using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly MongoDbContext _context;

    public PolicyRepository(MongoDbContext context) => _context = context;

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Policies
            .Find(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Policy?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Policy>.Filter.And(
            Builders<Policy>.Filter.Eq("number.value", policyNumber),
            Builders<Policy>.Filter.Eq(p => p.IsDeleted, false));

        return await _context.Policies.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Policy?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var keyDoc = await _context.PolicyIdempotencyKeys
            .Find(Builders<BsonDocument>.Filter.Eq("key", idempotencyKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (keyDoc is null) return null;

        var policyId = keyDoc["policyId"].AsGuid;
        return await GetByIdAsync(policyId, cancellationToken);
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
        var filter = BuildFilter(status, type, documentId, startDate, endDate);

        return await _context.Policies
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        PolicyStatus? status,
        PolicyType? type,
        string? documentId,
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, type, documentId, startDate, endDate);
        return (int)await _context.Policies.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await _context.Policies.InsertOneAsync(policy, cancellationToken: cancellationToken);
        await _context.PublishDomainEventsAsync(policy, cancellationToken);
    }

    public void SetIdempotencyKey(Policy policy, string idempotencyKey)
    {
        // Fire-and-forget insert; the caller always does SaveChangesAsync after this.
        // We store the key asynchronously — safe because AddAsync already persisted the policy.
        var doc = new BsonDocument
        {
            ["key"] = idempotencyKey,
            ["policyId"] = policy.Id.ToString()
        };
        _ = _context.PolicyIdempotencyKeys.InsertOneAsync(doc);
    }

    public async Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await _context.Policies.ReplaceOneAsync(
            p => p.Id == policy.Id,
            policy,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);

        await _context.PublishDomainEventsAsync(policy, cancellationToken);
    }

    public async Task<long> GetNextSequenceAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "policies");
        var update = Builders<BsonDocument>.Update.Inc("seq", 1L);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _context.Counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result["seq"].AsInt64;
    }

    // ------------------------------------------------------------------
    private static FilterDefinition<Policy> BuildFilter(
        PolicyStatus? status, PolicyType? type, string? documentId,
        DateOnly? startDate, DateOnly? endDate)
    {
        var filters = new List<FilterDefinition<Policy>>
        {
            Builders<Policy>.Filter.Eq(p => p.IsDeleted, false)
        };

        if (status.HasValue)
            filters.Add(Builders<Policy>.Filter.Eq(p => p.Status, status.Value));

        if (type.HasValue)
            filters.Add(Builders<Policy>.Filter.Eq(p => p.Type, type.Value));

        if (!string.IsNullOrWhiteSpace(documentId))
            filters.Add(Builders<Policy>.Filter.Eq("insured.documentId", documentId));

        if (startDate.HasValue)
            filters.Add(Builders<Policy>.Filter.Gte("coverage.startDate", startDate.Value.ToString("yyyy-MM-dd")));

        if (endDate.HasValue)
            filters.Add(Builders<Policy>.Filter.Lte("coverage.endDate", endDate.Value.ToString("yyyy-MM-dd")));

        return Builders<Policy>.Filter.And(filters);
    }
}
