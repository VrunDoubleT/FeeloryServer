namespace FeeloryBackend.Models.DTOs.Emote;

public class EmotePackageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public bool IsDefault { get; set; }

    public List<EmoteDto> Items { get; set; } = new();
}