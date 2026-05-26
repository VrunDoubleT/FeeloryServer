namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareFeedPagedDto
{
    public List<DayShareFeedItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}