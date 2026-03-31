using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InsuraTech.Domain.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace InsuraTech.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {
        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.ToTable("Claims");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedNever();

            builder.Property(c => c.Version)
               .IsConcurrencyToken();

            builder.Property(c => c.PolicyId)
                .IsRequired();

            builder.Property(c => c.PolicyId)
                .IsRequired();

            builder.Property(c => c.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(c => c.ClaimedAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(c => c.ApprovedAmount)
               .HasColumnType("decimal(18,2)");

            builder.Property(c => c.IncidentDate)
            .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(c => c.HasBeenAppealed)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(c => c.RejectionReason)
                .HasMaxLength(500);

            builder.Property(c => c.IsDeleted)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property( c => c.CreatedAt)
                .IsRequired();

            builder.HasMany(c => c.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
