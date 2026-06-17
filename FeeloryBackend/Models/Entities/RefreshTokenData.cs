namespace FeeloryBackend.Models.Entities;

public class RefreshTokenData
{
    public Guid UserId { get; set; }
    public DateTime ExpiredAt { get; set; }
    public long TokenVersion { get; set; }  // Used for bulk revocation on password change
}