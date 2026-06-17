namespace FeeloryBackend.Data.Configurations;

using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MissionTypeConfiguration
    : IEntityTypeConfiguration<MissionType>
{
    public void Configure(
        EntityTypeBuilder<MissionType> builder)
    {
        // Table mapping
        builder.ToTable("MissionTypes");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.MetricKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        // Unique metric key
        builder.HasIndex(x => x.MetricKey)
            .IsUnique();

        // Relationship: MissionType → Missions
        builder.HasMany(x => x.Missions)
            .WithOne(t => t.MissionType)
            .HasForeignKey(t => t.MissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}