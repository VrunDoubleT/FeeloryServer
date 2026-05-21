using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        // Table mapping
        builder.ToTable("Posts");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.MoodEmoteId).IsRequired();

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Privacy)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);
    
        builder.Property(p => p.DeletedAt)
            .IsRequired(false);
        
        // Index for feed query
        builder.HasIndex(x => x.UserId);

        // Relationship: Post → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Post → Emote (Mood)
        builder.HasOne(x => x.MoodEmote)
            .WithMany(e => e.Posts)
            .HasForeignKey(x => x.MoodEmoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Post → Viewers
        builder.HasMany(x => x.PostViewers)
            .WithOne(v => v.Post)
            .HasForeignKey(v => v.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Post → Feed
        builder.HasMany(x => x.PostFeeds)
            .WithOne(f => f.Post)
            .HasForeignKey(f => f.PostId)
            .OnDelete(DeleteBehavior.NoAction);

        // Relationship: Post → Reactions
        builder.HasMany(x => x.Reactions)
            .WithOne(r => r.Post)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Post → DaySharePosts
        builder.HasMany(x => x.DaySharePosts)
            .WithOne(d => d.Post)
            .HasForeignKey(d => d.PostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}