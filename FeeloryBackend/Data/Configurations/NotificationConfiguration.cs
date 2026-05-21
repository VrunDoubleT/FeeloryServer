using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Table mapping
        builder.ToTable("Notifications");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TypeId).IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .HasMaxLength(500);

        builder.Property(x => x.DataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsRead)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Optional read timestamp
        builder.Property(x => x.ReadAt);

        // Index for unread notifications
        builder.HasIndex(x => new { x.UserId, x.IsRead });

        // Relationship: Notification → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Notification → NotificationType
        builder.HasOne(x => x.Type)
            .WithMany(t => t.Notifications)
            .HasForeignKey(x => x.TypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}