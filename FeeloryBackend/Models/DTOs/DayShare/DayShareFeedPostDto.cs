namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareFeedPostDto
{
    public Guid Id { get; set; }

    public string? ImageUrl { get; set; }

    public DayShareMoodEmoteDto? MoodEmote { get; set; }
}