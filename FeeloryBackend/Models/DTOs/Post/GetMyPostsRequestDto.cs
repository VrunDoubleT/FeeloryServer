namespace FeeloryBackend.Models.DTOs.Post;

public class GetMyPostsRequestDto
{
    public DateTime? Date { get; set; }
    public string? Privacy { get; set; }
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
}