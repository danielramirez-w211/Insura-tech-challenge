using InsuraTech.Domain.Users;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Find(u => u.Id == id && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await _context.Users
            .Find(u => u.Email == email.ToLowerInvariant() && !u.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> ExistsAnyAsync(CancellationToken cancellationToken = default) =>
        await _context.Users.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken) > 0;

    public async Task<IReadOnlyList<User>> GetByLeaderIdAsync(Guid leaderId, CancellationToken cancellationToken = default)
    {
        var results = await _context.Users
            .Find(u => u.LeaderId == leaderId && !u.IsDeleted)
            .ToListAsync(cancellationToken);
        return results.AsReadOnly();
    }

    public async Task<IReadOnlyList<User>> GetAllLeadersAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.Users
            .Find(u => u.Role == Domain.Users.Role.Leader && !u.IsDeleted)
            .ToListAsync(cancellationToken);
        return results.AsReadOnly();
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.ReplaceOneAsync(
            u => u.Id == user.Id,
            user,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Reutiliza el patrón de contadores existente en MongoDB.
    /// Retorna el siguiente código de asesor formateado como "0001".
    /// </summary>
    public async Task<string> GetNextAdvisorCodeAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", "advisor_code");
        var update = Builders<BsonDocument>.Update.Inc("seq", 1L);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert       = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await _context.Counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        var seq = result["seq"].AsInt64;
        return seq.ToString("D4");
    }
}
