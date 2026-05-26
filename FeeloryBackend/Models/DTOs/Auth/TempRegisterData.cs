namespace FeeloryBackend.Models.DTOs.Auth;

public class TempRegisterData
{
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}
