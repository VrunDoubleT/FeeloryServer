namespace FeeloryBackend.Data.Configurations;

using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MissionRewardConfiguration
    : IEntityTypeConfiguration<MissionReward>
{
    public void Configure(
        EntityTypeBuilder<MissionReward> builder)
    {
        // Table mapping
        builder.ToTable("MissionRewards");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.MissionId)
            .IsRequired();

        builder.Property(x => x.PackageId)
            .IsRequired();

        // Prevent duplicate rewards
        builder.HasIndex(x => new
        {
            x.MissionId,
            x.PackageId
        }).IsUnique();

        // Relationship: MissionReward → Mission
        builder.HasOne(x => x.Mission)
            .WithMany(t => t.Rewards)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: MissionReward → Package
        builder.HasOne(x => x.Package)
            .WithMany(x => x.MissionRewards)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}