namespace FeeloryBackend.Models.DTOs.DayShare;

public class CreateDayShareRequestDto
{
    public DateOnly Date { get; set; }
    public List<Guid> PostIds { get; set; } = new();
    public List<Guid>? AllowedUserIds { get; set; }
}