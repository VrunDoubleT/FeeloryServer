namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareFeedItemDto
{
    public Guid DayShareId { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public DayShareOwnerDto Owner { get; set; } = null!;
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DaySharePostBasicDto> Posts { get; set; } = new();
}