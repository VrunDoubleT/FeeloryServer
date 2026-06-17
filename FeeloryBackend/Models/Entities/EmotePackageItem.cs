namespace FeeloryBackend.Models.Entities;

    public class EmotePackageItem
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public Guid EmoteId { get; set; }
     
        // Navigation properties
        public EmotePackage Package { get; set; } = null!;
        public Emote Emote { get; set; } = null!;
    }