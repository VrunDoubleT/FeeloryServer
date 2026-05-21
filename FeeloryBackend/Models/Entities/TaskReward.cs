namespace FeeloryBackend.Models.Entities;

public class TaskReward
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid PackageId { get; set; }
 
    // Navigation properties
    public Task Task { get; set; } = null!;
    public EmotePackage Package { get; set; } = null!;
}