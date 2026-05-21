namespace FeeloryBackend.Responses;

public class RefreshTokenResponse
{
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiredAt { get; set; }
}