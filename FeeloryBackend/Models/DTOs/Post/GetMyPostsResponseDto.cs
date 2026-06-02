namespace FeeloryBackend.Models.DTOs.Post;

public class GetMyPostsResponseDto
{
    public List<MyPostItemDto> Items { get; set; } = [];
    public int Total { get; set; }
    public string? NextCursor { get; set; }
}