using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        // Map to table
        builder.ToTable("FriendRequests");

        // Primary key
        builder.HasKey(x => x.Id);

        // Sender foreign key (required)
        builder.Property(x => x.SenderId)
            .IsRequired();

        // Receiver foreign key (required)
        builder.Property(x => x.ReceiverId)
            .IsRequired();

        // Status field (Pending / Accepted / Rejected)
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20);

        // Created timestamp
        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Index for fast query by sender
        builder.HasIndex(x => x.SenderId);

        // Index for fast query by receiver
        builder.HasIndex(x => x.ReceiverId);

        // Relationship: Sender → User
        builder.HasOne(x => x.Sender)
            .WithMany(u => u.SentFriendRequests)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Receiver → User
        builder.HasOne(x => x.Receiver)
            .WithMany(u => u.ReceivedFriendRequests)
            .HasForeignKey(x => x.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}