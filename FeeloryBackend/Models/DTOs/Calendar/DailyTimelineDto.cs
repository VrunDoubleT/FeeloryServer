using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Models.DTOs.Calendar;

public class DailyTimelineDto
{
    public DateOnly Date { get; set; }
    public List<PostDto> Posts { get; set; } = new();
}