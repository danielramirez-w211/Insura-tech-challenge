using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuraTech.Infrastructure.Persistence.Configurations
{
    public sealed class PolicyStatusHistoryConfiguration : IEntityTypeConfiguration<PolicyStatusHistory>
    {
        public void Configure(EntityTypeBuilder<PolicyStatusHistory> builder) 
        {
            builder.ToTable("PolicyStatusHistory");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Id)
                .ValueGeneratedNever();

            builder.Property(h => h.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(h => h.Notes)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(h => h.ChangedAt)
                .IsRequired();
        }
    }
}
