using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Notifications;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Users;
using InsuraTech.Infrastructure.Persistence.Documents;
using InsuraTech.Infrastructure.Persistence.IndexManagement;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IPublisher _publisher;

    public IMongoCollection<Policy>       Policies              => _database.GetCollection<Policy>("policies");
    public IMongoCollection<Claim>        Claims                => _database.GetCollection<Claim>("claims");
    public IMongoCollection<Notification> Notifications         => _database.GetCollection<Notification>("notifications");
    public IMongoCollection<User>         Users                 => _database.GetCollection<User>("users");
    public IMongoCollection<CityDocument> Cities                => _database.GetCollection<CityDocument>("cities");
    public IMongoCollection<BsonDocument> PolicyIdempotencyKeys => _database.GetCollection<BsonDocument>("policy_idempotency_keys");
    public IMongoCollection<BsonDocument> Counters              => _database.GetCollection<BsonDocument>("counters");

    public IMongoClient Client { get; }

    public MongoDbContext(IMongoClient client, string databaseName, IPublisher publisher)
    {
        Client    = client;
        _database = client.GetDatabase(databaseName);
        _publisher = publisher;
    }

    public async Task PublishDomainEventsAsync(AggregateRoot aggregate, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);

        aggregate.ClearDomainEvent();
    }

    public Task EnsureIndexesAsync() => IndexManager.EnsureAllAsync(this);
}
