namespace FeeloryBackend.Models.Entities;

public class Emote
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    
    // Navigation properties
    public ICollection<EmotePackageItem> EmotePackageItems { get; set; } = new List<EmotePackageItem>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}