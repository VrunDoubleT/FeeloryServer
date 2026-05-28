namespace FeeloryBackend.Models.DTOs.Post;

public class GetFriendFeedResponseDto
{
    public List<FriendFeedItemDto> Items { get; set; } = [];
    public string? NextCursor { get; set; }
}