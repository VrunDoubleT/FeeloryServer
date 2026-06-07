namespace FeeloryBackend.Data.Configurations;

using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MissionConfiguration
    : IEntityTypeConfiguration<Mission>
{
    public void Configure(
        EntityTypeBuilder<Mission> builder)
    {
        // Table mapping
        builder.ToTable("Missions");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.MissionTypeId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.TargetValue)
            .IsRequired();

        builder.Property(x => x.DurationDays)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        // Index for mission type filtering
        builder.HasIndex(x => x.MissionTypeId);

        // Relationship: Mission → MissionType
        builder.HasOne(x => x.MissionType)
            .WithMany(t => t.Missions)
            .HasForeignKey(x => x.MissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Mission → Rewards
        builder.HasMany(x => x.Rewards)
            .WithOne(r => r.Mission)
            .HasForeignKey(r => r.MissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Mission → UserMissions
        builder.HasMany(x => x.UserMissions)
            .WithOne(u => u.Mission)
            .HasForeignKey(u => u.MissionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relationship: Mission → ReactionHistories
        builder.HasMany(x => x.ReactionHistories)
            .WithOne(x => x.Mission)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}