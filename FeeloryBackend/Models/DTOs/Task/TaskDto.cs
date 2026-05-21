namespace FeeloryBackend.Models.DTOs.Task;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int TargetValue { get; set; }
}