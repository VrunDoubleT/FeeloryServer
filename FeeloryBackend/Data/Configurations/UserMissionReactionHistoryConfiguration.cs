using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserMissionReactionHistoryConfiguration
    : IEntityTypeConfiguration<UserMissionReactionHistory>
{
    public void Configure(
        EntityTypeBuilder<UserMissionReactionHistory> builder)
    {
        // Table mapping
        builder.ToTable("UserMissionReactionHistories");

        // Primary key
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.MissionId)
            .IsRequired();

        builder.Property(x => x.PostId)
            .IsRequired();

        builder.Property(x => x.ReactorId);
        
        // Prevent counting the same post twice
        builder.HasIndex(x => new
        {
            x.UserId,
            x.MissionId,
            x.PostId,
            x.ReactorId
        }).IsUnique();

        // Relationship: History → User
        builder.HasOne(x => x.User)
            .WithMany(x => x.MissionReactionHistories)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: History → Mission
        builder.HasOne(x => x.Mission)
            .WithMany(x => x.ReactionHistories)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: History → Post
        builder.HasOne(x => x.Post)
            .WithMany(x => x.ReactionMissionHistories)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relationship: History -> Reactor
        builder.HasOne(x => x.Reactor)
            .WithMany(x => x.ReactedMissionHistories)
            .HasForeignKey(x => x.ReactorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}