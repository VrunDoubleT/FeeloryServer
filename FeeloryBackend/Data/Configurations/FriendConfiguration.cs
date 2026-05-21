using FeeloryBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeeloryBackend.Data.Configurations;

public class FriendConfiguration : IEntityTypeConfiguration<Friend>
{
    public void Configure(EntityTypeBuilder<Friend> builder)
    {
        // Table
        builder.ToTable("Friends");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.FriendId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Indexing helps to query friend lists faster
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.FriendId);

        // Unique constraint: Do not allow duplicate relationship pairs
        builder.HasIndex(x => new { x.UserId, x.FriendId })
            .IsUnique();

        // Check constraint (Canonical order is guaranteed from the factory)
        builder.ToTable(t => t.HasCheckConstraint(
            "CHK_Friends_CanonicalOrder",
            "UserId < FriendId"
        ));

        // Relationship 1: User (side A)
        builder.HasOne(x => x.User)
            .WithMany(u => u.FriendsAsUser)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship 2: FriendUser (side B)
        builder.HasOne(x => x.FriendUser)
            .WithMany(u => u.FriendsAsFriend)
            .HasForeignKey(x => x.FriendId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}