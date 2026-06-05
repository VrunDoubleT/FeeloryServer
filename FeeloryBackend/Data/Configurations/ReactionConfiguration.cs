using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        // Table mapping
        builder.ToTable("Reactions");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EmoteId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        // Prevent duplicate reaction (1 user = 1 reaction per post)
        builder.HasIndex(x => new { x.PostId, x.UserId })
            .IsUnique();
        
        // Relationship: Reaction → Post
        builder.HasOne(x => x.Post)
            .WithMany(p => p.Reactions)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Relationship: Reaction → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.Reactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Reaction → Emote
        builder.HasOne(x => x.Emote)
            .WithMany(e => e.Reactions)
            .HasForeignKey(x => x.EmoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}