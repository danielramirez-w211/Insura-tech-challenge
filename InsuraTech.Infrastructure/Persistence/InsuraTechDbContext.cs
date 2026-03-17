using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Claims;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Policies;
using Microsoft.EntityFrameworkCore;

namespace InsuraTech.Infrastructure.Persistence
{
    public sealed class InsuraTechDbContext : DbContext
    {
        public InsuraTechDbContext(DbContextOptions<InsuraTechDbContext> options) : base(options) { }

        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<PolicyStatusHistory> PolicyStatusHistories => Set<PolicyStatusHistory>();
        public DbSet<Claim> Claims => Set<Claim>();
        public DbSet<ClaimStatusHistory> ClaimStatusHistories => Set<ClaimStatusHistory>();

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
                aggregate.ClearDomainEvent();

            return result;
        }
    }
}
