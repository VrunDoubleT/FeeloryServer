using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationTypeConfiguration : IEntityTypeConfiguration<NotificationType>
{
    public void Configure(EntityTypeBuilder<NotificationType> builder)
    {
        // Table mapping
        builder.ToTable("NotificationTypes");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        // Unique code for type
        builder.HasIndex(x => x.Code)
            .IsUnique();

        // Relationship: Type → Notifications
        builder.HasMany(x => x.Notifications)
            .WithOne(n => n.Type)
            .HasForeignKey(n => n.TypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}