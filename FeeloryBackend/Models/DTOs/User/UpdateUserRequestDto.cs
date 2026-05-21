namespace FeeloryBackend.Models.DTOs.User;

public class UpdateUserRequestDto
{
    public string DisplayName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
}