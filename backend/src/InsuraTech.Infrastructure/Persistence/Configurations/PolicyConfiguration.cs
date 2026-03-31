using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuraTech.Infrastructure.Persistence.Configurations
{
    public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.ToTable("Polices");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .ValueGeneratedNever();

            builder.Property(p => p.Version).IsConcurrencyToken();

            builder.OwnsOne(p => p.Number, nb =>
            {
                nb.Property(n => n.Value)
                .HasColumnName("PolicyNumber")
                .HasMaxLength(20)
                .IsRequired();

                nb.HasIndex(n => n.Value)
                .IsUnique();

            });
            builder.OwnsOne(p => p.Insured, ib =>
            {
                ib.Property(i => i.FullName)
                    .HasColumnName("InsuredFullName")
                    .HasMaxLength(200)
                    .IsRequired();

                ib.Property(i => i.DocumentId)
                    .HasColumnName("InsuredDocumentId")
                    .HasMaxLength(50)
                    .IsRequired();

                ib.Property(i => i.BirthDate)
                    .HasColumnName("InsuredBirthDate")
                    .IsRequired();
            });
            builder.OwnsOne(p => p.Coverage, cb =>
            {
                cb.Property(c => c.StartDate)
                    .HasColumnName("CoverageStartDate")
                    .IsRequired();

                cb.Property(c => c.EndDate)
                    .HasColumnName("CoverageEndDate")
                    .IsRequired();
            });

            builder.Property(p => p.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.MonthlyPremium)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.InsuredAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.AvailableInsuredAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.CancellationReason)
                .HasMaxLength(500);

            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            // Idempotency Key
            builder.Property<string>("IdempotencyKey")
                .HasMaxLength(100)
                .HasDefaultValue(null);

            builder.HasIndex("IdempotencyKey")
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");

            // Relación con StatusHistory
            builder.HasMany(p => p.StatusHistory)
                .WithOne()
                .HasForeignKey(h => h.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Soft delete filter
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}

