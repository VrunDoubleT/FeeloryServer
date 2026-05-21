using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmoteConfiguration : IEntityTypeConfiguration<Emote>
{
    public void Configure(EntityTypeBuilder<Emote> builder)
    {
        // Table mapping
        builder.ToTable("Emotes");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        // Index for fast lookup by name
        builder.HasIndex(x => x.Name)
            .IsUnique();

        // Relationship: Emote → Packages
        builder.HasMany(x => x.EmotePackageItems)
            .WithOne(x => x.Emote)
            .HasForeignKey(x => x.EmoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Emote → Posts
        builder.HasMany(x => x.Posts)
            .WithOne(p => p.MoodEmote)
            .HasForeignKey(p => p.MoodEmoteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Emote → Reactions
        builder.HasMany(x => x.Reactions)
            .WithOne(r => r.Emote)
            .HasForeignKey(r => r.EmoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}