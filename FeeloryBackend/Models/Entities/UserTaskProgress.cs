namespace FeeloryBackend.Models.Entities;

public class UserTaskProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TaskId { get; set; }
    public int CurrentValue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
 
    // Navigation properties
    public User User { get; set; } = null!;
    public Task Task { get; set; } = null!;
}