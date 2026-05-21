using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeeloryBackend.Data.Configurations;

public class DayShareFeedConfiguration : IEntityTypeConfiguration<DayShareFeed>
{
    public void Configure(EntityTypeBuilder<DayShareFeed> builder)
    {
        builder.ToTable("DayShareFeeds");
        
        builder.HasKey(dsf => dsf.Id);
        
        builder.Property(dsf => dsf.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();
            
        builder.Property(dsf => dsf.PostedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        // Relationships
        builder.HasOne(dsf => dsf.DayShare)
            .WithMany(ds => ds.DayShareFeeds)
            .HasForeignKey(dsf => dsf.DayShareId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(dsf => dsf.Viewer)
            .WithMany()
            .HasForeignKey(dsf => dsf.ViewerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}