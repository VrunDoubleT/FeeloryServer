namespace FeeloryBackend.Models.Entities;

public class Task
{
    public Guid Id { get; set; }
    public Guid TaskTypeId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int TargetValue { get; set; }
 
    // Navigation properties
    public TaskType TaskType { get; set; } = null!;
    public ICollection<TaskReward> Rewards { get; set; } = new List<TaskReward>();
    public ICollection<UserTaskProgress> UserProgresses { get; set; } = new List<UserTaskProgress>();
}