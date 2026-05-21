using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DayShareViewerConfiguration : IEntityTypeConfiguration<DayShareViewer>
{
    public void Configure(EntityTypeBuilder<DayShareViewer> builder)
    {
        // Table mapping
        builder.ToTable("DayShareViewers");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.DayShareId).IsRequired();
        builder.Property(x => x.ViewerId).IsRequired();

        // Prevent duplicate viewer per DayShare
        builder.HasIndex(x => new { x.DayShareId, x.ViewerId })
            .IsUnique();

        // Relationship: DayShareViewer → DayShare
        builder.HasOne(x => x.DayShare)
            .WithMany(d => d.DayShareViewers)
            .HasForeignKey(x => x.DayShareId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Viewer (User)
        builder.HasOne(x => x.Viewer)
            .WithMany(u => u.DayShareViewers)
            .HasForeignKey(x => x.ViewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}