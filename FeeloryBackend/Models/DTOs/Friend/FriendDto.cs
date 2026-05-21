using FeeloryBackend.Models.DTOs.Auth;

namespace FeeloryBackend.Models.DTOs.Friend;

public class FriendDto
{
    public Guid Id { get; set; }
    public UserDto User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}