namespace FeeloryBackend.Models.DTOs.Emote;

public class UserEmoteDto
{
    public EmoteDto Emote { get; set; } = null!;
    public DateTime? UnlockedAt { get; set; }
}