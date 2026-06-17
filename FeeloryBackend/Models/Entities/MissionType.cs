namespace FeeloryBackend.Models.Entities;

public class MissionType
{
    public Guid Id { get; set; }

    public string MetricKey { get; set; } = null!;

    public string Name { get; set; } = null!;
    
    // Navigation property
    public ICollection<Mission> Missions { get; set; } = new List<Mission>();
}