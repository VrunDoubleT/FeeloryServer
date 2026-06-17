namespace FeeloryBackend.Models.Entities;

public class EmotePackage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverUrl { get; set; }
    public bool IsDefault { get; set; }
    
    // Navigation properties
    public ICollection<EmotePackageItem> Items { get; set; } = new List<EmotePackageItem>();
    public ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
    public ICollection<MissionReward> MissionRewards { get; set; } = new List<MissionReward>();
}