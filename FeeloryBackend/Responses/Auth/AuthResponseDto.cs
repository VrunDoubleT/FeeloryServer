namespace FeeloryBackend.Models.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpiredAt { get; set; }
}
