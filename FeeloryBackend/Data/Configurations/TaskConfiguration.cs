using Task = FeeloryBackend.Models.Entities.Task;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskConfiguration : IEntityTypeConfiguration<Models.Entities.Task>
{
    public void Configure(EntityTypeBuilder<Models.Entities.Task> builder)
    {
        // Table mapping
        builder.ToTable("Tasks");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.TaskTypeId).IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.TargetValue)
            .IsRequired();

        // Index for task type filtering
        builder.HasIndex(x => x.TaskTypeId);

        // Relationship: Task → TaskType
        builder.HasOne(x => x.TaskType)
            .WithMany(t => t.Tasks)
            .HasForeignKey(x => x.TaskTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Task → Rewards
        builder.HasMany(x => x.Rewards)
            .WithOne(r => r.Task)
            .HasForeignKey(r => r.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Task → UserProgress
        builder.HasMany(x => x.UserProgresses)
            .WithOne(p => p.Task)
            .HasForeignKey(p => p.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }   
}