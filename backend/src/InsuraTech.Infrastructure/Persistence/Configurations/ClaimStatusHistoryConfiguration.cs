using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuraTech.Infrastructure.Persistence.Configurations
{
    public sealed class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
    {
        public void Configure(EntityTypeBuilder<ClaimStatusHistory> builder)
        {
            builder.ToTable("ClaimStatusHistory");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Id)
                .ValueGeneratedNever();

            builder.Property(h => h.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(h => h.ResponsibleUser)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(h => h.Observations)
                .HasMaxLength(500);

            builder.Property(h => h.ChangedAt)
                .IsRequired();
        }

    }
}
