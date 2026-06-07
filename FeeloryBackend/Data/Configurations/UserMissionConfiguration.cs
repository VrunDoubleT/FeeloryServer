namespace FeeloryBackend.Data.Configurations;

using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserMissionConfiguration
    : IEntityTypeConfiguration<UserMission>
{
    public void Configure(
        EntityTypeBuilder<UserMission> builder)
    {
        // Table mapping
        builder.ToTable("UserMissions");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MissionId)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.ExpiredAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.RewardClaimedAt)
            .HasColumnType("datetime2");

        // One mission per user
        builder.HasIndex(x => new
        {
            x.UserId,
            x.MissionId
        }).IsUnique();

        // Relationship: UserMission → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.UserMissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: UserMission → Mission
        builder.HasOne(x => x.Mission)
            .WithMany(t => t.UserMissions)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}