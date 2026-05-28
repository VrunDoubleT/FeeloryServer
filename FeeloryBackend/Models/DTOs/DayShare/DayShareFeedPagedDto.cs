namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareFeedPagedDto
{
    public List<DayShareFeedItemDto> Items { get; set; } = new();
   public string? NextCursor { get; set; } 
    public bool HasMore { get; set; }
}