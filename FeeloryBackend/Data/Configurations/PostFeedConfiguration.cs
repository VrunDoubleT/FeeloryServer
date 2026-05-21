using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PostFeedConfiguration : IEntityTypeConfiguration<PostFeed>
{
    public void Configure(EntityTypeBuilder<PostFeed> builder)
    {
        // Table mapping
        builder.ToTable("PostFeeds");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.ViewerId).IsRequired();
        builder.Property(x => x.PostedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Index for fast feed query by viewer
        builder.HasIndex(x => x.ViewerId);

        // Index for sorting feed by time
        builder.HasIndex(x => new { x.ViewerId, x.PostedAt });

        // Relationship: PostFeed → Post
        builder.HasOne(x => x.Post)
            .WithMany(p => p.PostFeeds)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: PostFeed → Viewer (User)
        builder.HasOne(x => x.Viewer)
            .WithMany(u => u.PostFeeds)
            .HasForeignKey(x => x.ViewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}