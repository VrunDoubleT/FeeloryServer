using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TaskTypeConfiguration : IEntityTypeConfiguration<TaskType>
{
    public void Configure(EntityTypeBuilder<TaskType> builder)
    {
        // Table mapping
        builder.ToTable("TaskTypes");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.MetricKey)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        // Unique metric key (used for system logic)
        builder.HasIndex(x => x.MetricKey)
            .IsUnique();

        // Relationship: TaskType → Tasks
        builder.HasMany(x => x.Tasks)
            .WithOne(t => t.TaskType)
            .HasForeignKey(t => t.TaskTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}