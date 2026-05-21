namespace FeeloryBackend.Models.DTOs.Emote;

public class EmoteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
}