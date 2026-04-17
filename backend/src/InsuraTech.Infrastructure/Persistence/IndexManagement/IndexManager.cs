using InsuraTech.Domain.Notifications;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Users;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.IndexManagement;

public static class IndexManager
{
    public static async Task EnsureAllAsync(MongoDbContext context)
    {
        await EnsurePolicyIndexesAsync(context);
        await EnsureNotificationIndexesAsync(context);
        await EnsureIdempotencyKeyIndexesAsync(context);
        await EnsureUserIndexesAsync(context);
    }

    private static Task EnsurePolicyIndexesAsync(MongoDbContext context) =>
        context.Policies.Indexes.CreateOneAsync(
            new CreateIndexModel<Policy>(
                Builders<Policy>.IndexKeys.Ascending("number.value"),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: CancellationToken.None);

    private static Task EnsureNotificationIndexesAsync(MongoDbContext context) =>
        context.Notifications.Indexes.CreateOneAsync(
            new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending("status")),
            cancellationToken: CancellationToken.None);

    private static Task EnsureIdempotencyKeyIndexesAsync(MongoDbContext context) =>
        context.PolicyIdempotencyKeys.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                new BsonDocument("key", 1),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: CancellationToken.None);

    private static Task EnsureUserIndexesAsync(MongoDbContext context) =>
        context.Users.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }),
            cancellationToken: CancellationToken.None);
}
