using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmotePackageConfiguration : IEntityTypeConfiguration<EmotePackage>
{
    public void Configure(EntityTypeBuilder<EmotePackage> builder)
    {
        // Table mapping
        builder.ToTable("EmotePackages");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.CoverUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsDefault)
            .IsRequired();

        // Index for default package
        builder.HasIndex(x => x.IsDefault);

        // Relationship: Package → Items
        builder.HasMany(x => x.Items)
            .WithOne(i => i.Package)
            .HasForeignKey(i => i.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Package → UserPackages
        builder.HasMany(x => x.UserPackages)
            .WithOne(u => u.Package)
            .HasForeignKey(u => u.PackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Package → TaskRewards
        builder.HasMany(x => x.TaskRewards)
            .WithOne(t => t.Package)
            .HasForeignKey(t => t.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}