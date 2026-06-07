namespace FeeloryBackend.Models.Entities;

public class MissionReward
{
    public Guid Id { get; set; }

    public Guid MissionId { get; set; }

    public Guid PackageId { get; set; }

    public Mission Mission { get; set; } = null!;

    public EmotePackage Package { get; set; } = null!;
    
    // Navigator 
    public ICollection<EmotePackage> EmotePackages = new List<EmotePackage>();
}