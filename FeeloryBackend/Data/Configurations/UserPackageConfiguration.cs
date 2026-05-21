using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserPackageConfiguration : IEntityTypeConfiguration<UserPackage>
{
    public void Configure(EntityTypeBuilder<UserPackage> builder)
    {
        // Table mapping
        builder.ToTable("UserPackages");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.PackageId).IsRequired();
        builder.Property(x => x.UnlockedAt).IsRequired();

        // Prevent duplicate unlock of same package for same user
        builder.HasIndex(x => new { x.UserId, x.PackageId })
            .IsUnique();

        // Index for querying user owned packages
        builder.HasIndex(x => x.UserId);

        // Relationship: UserPackage → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.UserPackages)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: UserPackage → EmotePackage
        builder.HasOne(x => x.Package)
            .WithMany(p => p.UserPackages)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}