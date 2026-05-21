using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Models.DTOs.Feed;

public class FeedDto
{
    public List<PostDto> Posts { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}