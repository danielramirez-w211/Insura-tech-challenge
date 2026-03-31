using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Notifications;
using InsuraTech.Domain.Policies;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InsuraTech.Infrastructure.Persistence
{
    public sealed class InsuraTechDbContext : DbContext
    {
        private readonly IPublisher _publisher;

        public InsuraTechDbContext(DbContextOptions<InsuraTechDbContext> options, IPublisher publisher)
            : base(options)
        {
            _publisher = publisher;
        }

        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<PolicyStatusHistory> PolicyStatusHistories => Set<PolicyStatusHistory>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<ClaimStatusHistory> ClaimStatusHistories => Set<ClaimStatusHistory>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuraTechDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var aggregates = ChangeTracker
                .Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var aggregate in aggregates)
            {
                foreach (var domainEvent in aggregate.DomainEvents)
                    await _publisher.Publish(domainEvent, cancellationToken);

                aggregate.ClearDomainEvent();
            }

            return result;
        }
    }
}
