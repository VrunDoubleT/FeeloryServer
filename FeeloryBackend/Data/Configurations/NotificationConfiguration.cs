using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeeloryBackend.Data.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // Table mapping
        builder.ToTable("Notifications");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.TargetId);

        builder.Property(x => x.DataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.ReadAt);

        // Indexes
        builder.HasIndex(x => new { x.UserId, x.IsRead });

        builder.HasIndex(x => x.CreatedAt);

        builder.HasIndex(x => x.Type);

        // Relationship: Notification -> User (Receiver)
        builder.HasOne(x => x.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: Notification -> User (Actor)
        builder.HasOne(x => x.Actor)
            .WithMany()
            .HasForeignKey(x => x.ActorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}