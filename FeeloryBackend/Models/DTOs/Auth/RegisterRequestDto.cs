namespace FeeloryBackend.Models.DTOs.Auth;

public class RegisterRequestDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
}