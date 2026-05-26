namespace FeeloryBackend.Models.DTOs.Auth;

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
