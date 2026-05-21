using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PostViewerConfiguration : IEntityTypeConfiguration<PostViewer>
{
    public void Configure(EntityTypeBuilder<PostViewer> builder)
    {
        // Table mapping
        builder.ToTable("PostViewers");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.ViewerId).IsRequired();

        // Prevent duplicate view record
        builder.HasIndex(x => new { x.PostId, x.ViewerId })
            .IsUnique();

        // Relationship: Viewer → Post
        builder.HasOne(x => x.Post)
            .WithMany(p => p.PostViewers)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Viewer → User
        builder.HasOne(x => x.Viewer)
            .WithMany(u => u.PostViewers)
            .HasForeignKey(x => x.ViewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}