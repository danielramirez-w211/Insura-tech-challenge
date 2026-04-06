using InsuraTech.Application.Common.Interfaces;
using MongoDB.Driver;

namespace InsuraTech.Infrastructure.Persistence;

/// <summary>
/// MongoDB implementation of IUnitOfWork.
/// Single-document operations are atomic by default; multi-document operations
/// use a client session transaction.
/// Repository methods write immediately — SaveChangesAsync publishes domain events
/// that were queued during the operation (handled per-repo call).
/// </summary>
public sealed class MongoUnitOfWork : IUnitOfWork
{
    private readonly MongoDbContext _context;
    private IClientSessionHandle? _session;

    public MongoUnitOfWork(MongoDbContext context)
    {
        _context = context;
    }

    public IClientSessionHandle? Session => _session;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Repositories write immediately; this is a no-op kept for interface compatibility.
        return Task.FromResult(1);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null)
        {
            await _session.CommitTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_session is not null)
        {
            await _session.AbortTransactionAsync(cancellationToken);
            _session.Dispose();
            _session = null;
        }
    }
}
