namespace FeeloryBackend.Models.DTOs.Task;

public class MissionRewardDto
{
    public Guid PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public string? Description  { get; set; }
    public string? CoverUrl { get; set; }
}