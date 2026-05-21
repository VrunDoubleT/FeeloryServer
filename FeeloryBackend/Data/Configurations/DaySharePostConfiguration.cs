using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DaySharePostConfiguration : IEntityTypeConfiguration<DaySharePost>
{
    public void Configure(EntityTypeBuilder<DaySharePost> builder)
    {
        // Table mapping
        builder.ToTable("DaySharePosts");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.DayShareId).IsRequired();
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Index for sorting posts in a share
        builder.HasIndex(x => new { x.DayShareId, x.DisplayOrder });

        // Relationship: DaySharePost → DayShare
        builder.HasOne(x => x.DayShare)
            .WithMany(d => d.DaySharePosts)
            .HasForeignKey(x => x.DayShareId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: DaySharePost → Post
        builder.HasOne(x => x.Post)
            .WithMany(p => p.DaySharePosts)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}