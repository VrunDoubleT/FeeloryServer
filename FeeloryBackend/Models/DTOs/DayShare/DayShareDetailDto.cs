namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareDetailDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public string Privacy { get; set; } = string.Empty;
    public List<DaySharePostItemDto> Posts { get; set; } = new();
    public DayShareOwnerDto Owner { get; set; } = null!;
    
    public List<Guid> AllowedUserIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}