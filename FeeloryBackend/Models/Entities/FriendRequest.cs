using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.Entities;

public class FriendRequest
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Status { get; set; } = FriendRequestConstants.Pending;
    public DateTime CreatedAt { get; set; }
 
    // Navigation properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}