namespace FeeloryBackend.Models.DTOs.Task;

public class UserTaskProgressDto
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = null!;
    public int CurrentValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}