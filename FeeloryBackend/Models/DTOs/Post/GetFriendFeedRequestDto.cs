namespace FeeloryBackend.Models.DTOs.Post;

public class GetFriendFeedRequestDto
{
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
}