using FeeloryBackend.Models.DTOs.Auth;

namespace FeeloryBackend.Models.DTOs.Friend;

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public UserDto Sender { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}