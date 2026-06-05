namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareOwnerDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}