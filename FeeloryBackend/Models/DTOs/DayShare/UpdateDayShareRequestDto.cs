namespace FeeloryBackend.Models.DTOs.DayShare;

public class UpdateDayShareRequestDto
{
    public Guid DayShareId { get; set; }
    public string? Description { get; set; }
    public string Privacy { get; set; } = string.Empty;
    public List<Guid>? AllowedUserIds { get; set; }
}