using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserTaskProgressConfiguration : IEntityTypeConfiguration<UserTaskProgress>
{
    public void Configure(EntityTypeBuilder<UserTaskProgress> builder)
    {
        // Table mapping
        builder.ToTable("UserTaskProgresses");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TaskId).IsRequired();
        builder.Property(x => x.CurrentValue).IsRequired();
        builder.Property(x => x.IsCompleted).IsRequired();

        // Optional field
        builder.Property(x => x.CompletedAt);

        // Prevent duplicate progress record per user-task
        builder.HasIndex(x => new { x.UserId, x.TaskId })
            .IsUnique();

        // Index for leaderboard / task filtering
        builder.HasIndex(x => x.TaskId);

        // Relationship: UserTaskProgress → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.TaskProgresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: UserTaskProgress → Task
        builder.HasOne(x => x.Task)
            .WithMany(t => t.UserProgresses)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}