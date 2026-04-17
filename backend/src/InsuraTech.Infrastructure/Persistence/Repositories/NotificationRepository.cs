using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Notifications;
using InsuraTech.Infrastructure.Persistence.Repositories.Base;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository : MongoRepository<Notification>, INotificationRepository
{
    public NotificationRepository(MongoDbContext context) : base(context) { }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Notifications
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
                NotDeleted())
            : NotDeleted();

        var totalCount = (int)await Context.Notifications.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var items = await ApplyPagination(
                Context.Notifications.Find(filter).SortByDescending(n => n.CreatedAt),
                page, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await Context.Notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await Context.Notifications.ReplaceOneAsync(
            n => n.Id == notification.Id,
            notification,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);
    }
}
