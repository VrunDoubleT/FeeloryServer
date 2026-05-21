using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskRewardConfiguration : IEntityTypeConfiguration<TaskReward>
{
    public void Configure(EntityTypeBuilder<TaskReward> builder)
    {
        // Table mapping
        builder.ToTable("TaskRewards");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.TaskId).IsRequired();
        builder.Property(x => x.PackageId).IsRequired();

        // Prevent duplicate reward mapping
        builder.HasIndex(x => new { x.TaskId, x.PackageId })
            .IsUnique();

        // Relationship: Reward → Task
        builder.HasOne(x => x.Task)
            .WithMany(t => t.Rewards)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Reward → EmotePackage
        builder.HasOne(x => x.Package)
            .WithMany(p => p.TaskRewards)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}