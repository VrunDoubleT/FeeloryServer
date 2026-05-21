namespace FeeloryBackend.Models.DTOs.Auth;

public class LoginRequestDto
{
    public string UsernameOrEmail { get; set; } = null!;
    public string Password { get; set; } = null!;
}