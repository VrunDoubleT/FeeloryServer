namespace FeeloryBackend.Models.DTOs.User;

public class UpdateUserRequestDto
{
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
}