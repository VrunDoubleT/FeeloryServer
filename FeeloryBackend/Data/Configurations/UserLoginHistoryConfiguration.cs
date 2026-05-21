using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserLoginHistoryConfiguration : IEntityTypeConfiguration<UserLoginHistory>
{
    public void Configure(EntityTypeBuilder<UserLoginHistory> builder)
    {
        // Table mapping
        builder.ToTable("UserLoginHistories");

        // Primary key
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.LoginDate).IsRequired();
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Index for analytics (login tracking)
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.LoginDate);

        // Relationship: LoginHistory → User
        builder.HasOne(x => x.User)
            .WithMany(u => u.LoginHistories)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}