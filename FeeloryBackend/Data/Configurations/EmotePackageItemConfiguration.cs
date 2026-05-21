using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmotePackageItemConfiguration : IEntityTypeConfiguration<EmotePackageItem>
{
    public void Configure(EntityTypeBuilder<EmotePackageItem> builder)
    {
        // Table mapping
        builder.ToTable("EmotePackageItems");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.PackageId).IsRequired();
        builder.Property(x => x.EmoteId).IsRequired();

        // Prevent duplicate emote inside same package
        builder.HasIndex(x => new { x.PackageId, x.EmoteId })
            .IsUnique();

        // Relationship: Item → Package
        builder.HasOne(x => x.Package)
            .WithMany(p => p.Items)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Item → Emote
        builder.HasOne(x => x.Emote)
            .WithMany(e => e.EmotePackageItems)
            .HasForeignKey(x => x.EmoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}