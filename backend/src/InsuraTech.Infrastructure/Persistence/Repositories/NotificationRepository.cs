using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly MongoDbContext _context;

    public NotificationRepository(MongoDbContext context) => _context = context;

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Find(n => n.Id == id && !n.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Notification> Items, int TotalCount)> GetAllAsync(
        NotificationStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = status.HasValue
            ? Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.Status, status.Value),
                Builders<Notification>.Filter.Eq(n => n.IsDeleted, false))
            : Builders<Notification>.Filter.Eq(n => n.IsDeleted, false);

        var totalCount = (int)await _context.Notifications.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await _context.Notifications
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.ReplaceOneAsync(
            n => n.Id == notification.Id,
            notification,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);
    }
}
