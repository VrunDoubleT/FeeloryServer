using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class DaySharePostBasicDto
{
    public Guid Id { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public EmoteDto? MoodEmote { get; set; }
    public DateTime CreatedAt { get; set; }
}