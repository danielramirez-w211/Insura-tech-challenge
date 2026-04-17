using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Infrastructure.Persistence.Repositories.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class PolicyRepository : MongoRepository<Policy>, IPolicyRepository
{
    public PolicyRepository(MongoDbContext context) : base(context) { }

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Policies
            .Find(p => p.Id == id && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Policy?> GetByNumberAsync(string policyNumber, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Policy>.Filter.And(
            Builders<Policy>.Filter.Eq("number.value", policyNumber),
            NotDeleted());

        return await Context.Policies.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Policy?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var keyDoc = await Context.PolicyIdempotencyKeys
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
        string? insuredSearch,
        string? insuredDocumentType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, type, documentId, startDate, endDate, insuredSearch, insuredDocumentType);

        return await ApplyPagination(
                Context.Policies.Find(filter).SortByDescending(p => p.CreatedAt),
                page, pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        PolicyStatus? status,
        PolicyType? type,
        string? documentId,
        DateOnly? startDate,
        DateOnly? endDate,
        string? insuredSearch,
        string? insuredDocumentType,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(status, type, documentId, startDate, endDate, insuredSearch, insuredDocumentType);
        return (int)await Context.Policies.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await Context.Policies.InsertOneAsync(policy, cancellationToken: cancellationToken);
        await Context.PublishDomainEventsAsync(policy, cancellationToken);
    }

    public void SetIdempotencyKey(Policy policy, string idempotencyKey)
    {
        // Fire-and-forget insert; safe because AddAsync already persisted the policy.
        var doc = new BsonDocument
        {
            ["key"]      = idempotencyKey,
            ["policyId"] = policy.Id.ToString()
        };
        _ = Context.PolicyIdempotencyKeys.InsertOneAsync(doc);
    }

    public async Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await Context.Policies.ReplaceOneAsync(
            p => p.Id == policy.Id,
            policy,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);

        await Context.PublishDomainEventsAsync(policy, cancellationToken);
    }

    public async Task<long> GetNextSequenceAsync(CancellationToken cancellationToken = default)
    {
        var filter  = Builders<BsonDocument>.Filter.Eq("_id", "policies");
        var update  = Builders<BsonDocument>.Update.Inc("seq", 1L);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert       = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await Context.Counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        return result["seq"].AsInt64;
    }

    public async Task<int> CountByAdvisorIdAsync(Guid advisorId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Policy>.Filter.And(
            Builders<Policy>.Filter.Eq(p => p.CreatedByAdvisorId, advisorId),
            NotDeleted());

        return (int)await Context.Policies.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<ClientSummaryProjection>> GetMyClientsAsync(
        Guid advisorId,
        CancellationToken cancellationToken = default)
    {
        var groupStage = new BsonDocument("$group", new BsonDocument
        {
            { "_id",          "$insured.DocumentId" },
            { "documentType", new BsonDocument("$first", "$insured.DocumentType") },
            { "firstName",    new BsonDocument("$first", "$insured.FirstName") },
            { "lastName",     new BsonDocument("$first", "$insured.LastName") },
            { "cityName",     new BsonDocument("$first", "$insured.CityName") },
            { "policyCount",  new BsonDocument("$sum", 1) }
        });

        var sortStage = new BsonDocument("$sort", new BsonDocument("lastName", 1));

        var results = await Context.Policies
            .Aggregate()
            .Match(p => p.CreatedByAdvisorId == (Guid?)advisorId && !p.IsDeleted)
            .AppendStage<BsonDocument>(groupStage)
            .AppendStage<BsonDocument>(sortStage)
            .ToListAsync(cancellationToken);

        return results.Select(doc => new ClientSummaryProjection(
            DocumentId:   doc["_id"].AsString,
            DocumentType: doc["documentType"].AsString,
            FirstName:    doc["firstName"].AsString,
            LastName:     doc["lastName"].AsString,
            CityName:     doc.Contains("cityName") && doc["cityName"] != BsonNull.Value
                              ? doc["cityName"].AsString
                              : string.Empty,
            PolicyCount:  doc["policyCount"].AsInt32
        ));
    }

    // ------------------------------------------------------------------
    private static FilterDefinition<Policy> BuildFilter(
        PolicyStatus? status, PolicyType? type, string? documentId,
        DateOnly? startDate, DateOnly? endDate,
        string? insuredSearch = null, string? insuredDocumentType = null)
    {
        var filters = new List<FilterDefinition<Policy>> { NotDeleted() };

        if (status.HasValue)
            filters.Add(Builders<Policy>.Filter.Eq(p => p.Status, status.Value));

        if (type.HasValue)
            filters.Add(Builders<Policy>.Filter.Eq(p => p.Type, type.Value));

        if (!string.IsNullOrWhiteSpace(documentId))
            filters.Add(Builders<Policy>.Filter.Eq("insured.DocumentId", documentId));

        if (startDate.HasValue)
            filters.Add(Builders<Policy>.Filter.Gte("coverage.startDate", startDate.Value.ToString("yyyy-MM-dd")));

        if (endDate.HasValue)
            filters.Add(Builders<Policy>.Filter.Lte("coverage.endDate", endDate.Value.ToString("yyyy-MM-dd")));

        if (!string.IsNullOrWhiteSpace(insuredSearch))
        {
            var escaped = System.Text.RegularExpressions.Regex.Escape(insuredSearch.Trim());
            var regex   = new BsonRegularExpression($"^{escaped}", "i");
            filters.Add(Builders<Policy>.Filter.Or(
                Builders<Policy>.Filter.Regex("insured.FirstName", regex),
                Builders<Policy>.Filter.Regex("insured.LastName",  regex)
            ));
        }

        if (!string.IsNullOrWhiteSpace(insuredDocumentType))
            filters.Add(Builders<Policy>.Filter.Eq("insured.DocumentType", insuredDocumentType));

        return Builders<Policy>.Filter.And(filters);
    }
}
