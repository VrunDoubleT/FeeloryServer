namespace FeeloryBackend.Models.DTOs.User;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}