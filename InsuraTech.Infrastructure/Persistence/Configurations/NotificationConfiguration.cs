using InsuraTech.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsuraTech.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.RecipientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Subject)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.FailureReason)
            .HasMaxLength(500);

        builder.Property(n => n.CorrelationId)
            .HasMaxLength(100);

        builder.Property(n => n.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.CreatedAt).IsRequired();

        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.RecipientId);

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}
