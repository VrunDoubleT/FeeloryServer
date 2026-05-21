using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DayShareConfiguration : IEntityTypeConfiguration<DayShare>
{
    public void Configure(EntityTypeBuilder<DayShare> builder)
    {
        // Table mapping
        builder.ToTable("DayShares");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.SharedDate)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.ShareType)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);
        
        builder.Property(x => x.DeletedAt)
            .IsRequired(false);

        // Index for querying shares by owner
        builder.HasIndex(x => x.OwnerId);

        // Relationship: Owner (User)
        builder.HasOne(x => x.Owner)
            .WithMany(u => u.DayShares)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: DayShare → Posts
        builder.HasMany(x => x.DaySharePosts)
            .WithOne(x => x.DayShare)
            .HasForeignKey(x => x.DayShareId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: DayShare → Viewers
        builder.HasMany(x => x.DayShareViewers)
            .WithOne(x => x.DayShare)
            .HasForeignKey(x => x.DayShareId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}