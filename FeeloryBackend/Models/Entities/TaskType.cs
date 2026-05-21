namespace FeeloryBackend.Models.Entities;

public class TaskType
{
    public Guid Id { get; set; }
    public string MetricKey { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
 
    // Navigation properties
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}