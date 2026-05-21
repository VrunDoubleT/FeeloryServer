namespace FeeloryBackend.Settings;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiresMinutes { get; set; }
    public int RefreshTokenExpiresDays { get; set; }
}